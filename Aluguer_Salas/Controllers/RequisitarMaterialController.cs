// Aluguer_Salas/Controllers/RequisitarMaterialController.cs
using Aluguer_Salas.Data;
using Aluguer_Salas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

[Authorize]
public class RequisitarMaterialController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Utilizador> _userManager;

    public RequisitarMaterialController(ApplicationDbContext context,
                                        UserManager<Utilizador> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task PopulateViewBagAsync()
    {
        Console.WriteLine("Controller: Iniciando PopulateViewBagAsync.");
        var materiais = await _context.Materiais
                                      .OrderBy(m => m.Nome)
                                      .ToListAsync();

        Console.WriteLine($"Controller: {materiais.Count} materiais encontrados no banco.");
        foreach (var mat in materiais)
        {
            Console.WriteLine($"Controller - Material para SelectList: Id={mat.Id}, Nome='{mat.Nome}'");
            if (string.IsNullOrEmpty(mat.Nome))
            {
                Console.WriteLine($"AVISO Controller: Material com Id={mat.Id} tem Nome nulo ou vazio!");
            }
        }

        ViewBag.ListaMateriaisDisponiveis = new SelectList(
            materiais,
            "Id",
            "Nome"
        );
        Console.WriteLine("Controller: ViewBag.ListaMateriaisDisponiveis populado.");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        Console.WriteLine("GET: RequisitarMaterial/Index iniciado.");
        await PopulateViewBagAsync();

        var viewModelParaPrimeiroItem = new RequisicaoMaterial
        {
            DataRequisicao = DateTime.Today,
        };

        // Se tiver lógica de Sala Pendente, popule o ViewBag aqui
        if (TempData.TryGetValue("SalaIdPendente", out object salaIdObj) && TempData.TryGetValue("NomeSalaPendente", out object nomeSalaObj))
        {
            if (salaIdObj is int salaId && nomeSalaObj is string nomeSala)
            {
                ViewBag.SalaIdPendente = salaId;
                ViewBag.NomeSalaPendente = nomeSala; // Usado no título da view
                Console.WriteLine($"Controller: Sala pendente Id={salaId}, Nome='{nomeSala}'");
            }
        }

        ViewData["Title"] = "Requisitar Material";
        Console.WriteLine("Controller: GET Index - Formulário de requisição de material preparado.");
        return View(viewModelParaPrimeiroItem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        DateTime dataRequisicao,
        TimeSpan horaInicio,
        TimeSpan horaFim,
        int[] materialId,
        int[] quantidadeRequisitada,
        int? salaIdPendenteInput) // Recebe o valor do hidden input
    {
        Console.WriteLine("POST: RequisitarMaterial/Index recebido para múltiplos itens.");

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            Console.WriteLine("Controller: Utilizador não autenticado no POST.");
            TempData["ErrorMessage"] = "Utilizador não autenticado. Por favor, faz login novamente.";
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }
        Console.WriteLine($"Controller: Utilizador autenticado: {currentUser.UserName} (Id: {currentUser.Id})");

        if (horaFim <= horaInicio)
        {
            ModelState.AddModelError(string.Empty, "A hora de fim deve ser posterior à hora de início.");
        }
        if (dataRequisicao.Date < DateTime.Today)
        {
            ModelState.AddModelError(string.Empty, "A data de requisição não pode ser no passado.");
        }

        if (materialId == null || quantidadeRequisitada == null || materialId.Length == 0 || materialId.Length != quantidadeRequisitada.Length)
        {
            ModelState.AddModelError(string.Empty, "Nenhum material foi selecionado ou os dados dos itens estão incompletos. Adicione pelo menos um item.");
        }
        else
        {
            for (int i = 0; i < materialId.Length; i++)
            {
                if (materialId[i] <= 0)
                {
                    ModelState.AddModelError($"ItemError_{i}", $"Item {i + 1}: Por favor, selecione um material.");
                }
                if (quantidadeRequisitada[i] <= 0)
                {
                    ModelState.AddModelError($"ItemError_{i}", $"Item {i + 1}: A quantidade deve ser pelo menos 1.");
                }
            }
        }

        var requisicoesParaSalvar = new List<RequisicaoMaterial>();
        if (ModelState.IsValid && materialId != null && materialId.Length > 0)
        {
            bool todosItensDisponiveis = true;
            for (int i = 0; i < materialId.Length; i++)
            {
                int currentMaterialId = materialId[i];
                int currentQuantidade = quantidadeRequisitada[i];

                if (currentMaterialId <= 0 || currentQuantidade <= 0) continue;

                var materialInfo = await _context.Materiais.FindAsync(currentMaterialId);
                if (materialInfo == null)
                {
                    ModelState.AddModelError($"ItemError_{i}", $"Item {i + 1}: Material não encontrado.");
                    todosItensDisponiveis = false;
                    continue;
                }

                var conflitos = await _context.RequisicoesMaterial
                    .Where(r => r.MaterialId == currentMaterialId &&
                                r.DataRequisicao.Date == dataRequisicao.Date &&
                                horaInicio < r.HoraFim &&
                                horaFim > r.HoraInicio)
                    .ToListAsync();

                int jaRequisitadoNoPeriodo = conflitos.Sum(r => r.QuantidadeRequisitada);
                int disponivelNoPeriodo = materialInfo.QuantidadeDisponivel - jaRequisitadoNoPeriodo;

                Console.WriteLine($"Controller: Verificando Material ID {currentMaterialId} ({materialInfo.Nome}). Total: {materialInfo.QuantidadeDisponivel}, Já requisitado no período: {jaRequisitadoNoPeriodo}, Disponível no período: {disponivelNoPeriodo}, Tentando requisitar: {currentQuantidade}");

                if (currentQuantidade > disponivelNoPeriodo)
                {
                    ModelState.AddModelError($"ItemError_{i}",
                        $"Item {i + 1} ({materialInfo.Nome}): Apenas {Math.Max(0, disponivelNoPeriodo)} unidade(s) estão disponíveis nesse horário. Você tentou requisitar {currentQuantidade}.");
                    todosItensDisponiveis = false;
                }
                else
                {
                    requisicoesParaSalvar.Add(new RequisicaoMaterial
                    {
                        UtilizadorId = currentUser.Id,
                        MaterialId = currentMaterialId,
                        QuantidadeRequisitada = currentQuantidade,
                        DataRequisicao = dataRequisicao.Date,
                        HoraInicio = horaInicio,
                        HoraFim = horaFim
                        // Aqui você adicionaria o SalaId se a requisição estiver ligada a uma sala específica
                        // Ex: SalaId = salaIdPendenteInput (após verificar se tem valor)
                    });
                }
            }
            if (!todosItensDisponiveis)
            {
                requisicoesParaSalvar.Clear();
            }
        }

        if (ModelState.IsValid && requisicoesParaSalvar.Any())
        {
            try
            {
                _context.RequisicoesMaterial.AddRange(requisicoesParaSalvar);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Controller: {requisicoesParaSalvar.Count} requisição(ões) de material guardada(s) com sucesso.");
                TempData["SuccessMessage"] = $"{requisicoesParaSalvar.Count} requisição(ões) de material efetuada(s) com sucesso!";
                return RedirectToAction("Salas", "Alugar"); // Ajuste o redirecionamento conforme necessário
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Controller: Erro DbUpdateException ao guardar - {ex.ToString()}");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao tentar guardar as requisições. Tente novamente ou contacte o suporte.");
            }
        }
        else
        {
            Console.WriteLine("Controller: ModelState inválido ou nenhum item válido para salvar. Requisições não foram guardadas.");
            if (!requisicoesParaSalvar.Any() && materialId != null && materialId.Length > 0 && ModelState.ErrorCount == 0)
            {
                if (!ModelState.Values.SelectMany(v => v.Errors).Any(e => e.ErrorMessage.Contains("disponíveis nesse horário")))
                {
                    ModelState.AddModelError(string.Empty, "Nenhum dos materiais selecionados estava disponível na quantidade ou período solicitados.");
                }
            }
            // Log detalhado dos erros do ModelState
            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                foreach (var error in modelStateVal.Errors)
                {
                    Console.WriteLine($"Controller - Erro de Validação: Chave='{modelStateKey}', Erro='{error.ErrorMessage}'");
                }
            }
        }

        await PopulateViewBagAsync();
        var modelParaRetorno = new RequisicaoMaterial
        {
            DataRequisicao = dataRequisicao,
            HoraInicio = horaInicio,
            HoraFim = horaFim
        };

        if (salaIdPendenteInput.HasValue)
        {
            ViewBag.SalaIdPendente = salaIdPendenteInput.Value;
            var salaPendenteInfo = await _context.Salas.FindAsync(salaIdPendenteInput.Value);
            if (salaPendenteInfo != null)
            {
                ViewBag.NomeSalaPendente = salaPendenteInfo.NomeSala;
            }
        }

        ViewData["Title"] = "Requisitar Material";
        Console.WriteLine("Controller: POST Index - Retornando View com erros de validação ou sem itens válidos.");
        return View(modelParaRetorno);
    }
}