using Aluguer_Salas.Data;
using Aluguer_Salas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class RequisitarMaterialController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Utilizador> _userManager;

    // O construtor recebe o ApplicationDbContext e o UserManager para acessar os dados do utilizador autenticado.
    public RequisitarMaterialController(ApplicationDbContext context, UserManager<Utilizador> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Método auxiliar para popular o ViewBag com a lista de materiais disponíveis.
    private async Task PopulateViewBagAsync()
    {
        var materiais = await _context.Materiais
                                      .OrderBy(m => m.Nome)
                                      .ToListAsync();
        ViewBag.ListaMateriaisDisponiveis = new SelectList(materiais, "Id", "Nome");
    }

    // Ação GET para exibir o formulário de requisição de material.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await PopulateViewBagAsync();

        var viewModelParaPrimeiroItem = new RequisicaoMaterial
        {
            DataRequisicao = DateTime.Today,
        };

        // Se houver uma sala pendente, recupera os dados da TempData e os adiciona ao ViewBag.
        if (TempData.TryGetValue("SalaIdPendente", out object salaIdObj) && TempData.TryGetValue("NomeSalaPendente", out object nomeSalaObj))
        {
            if (salaIdObj is int salaId && nomeSalaObj is string nomeSala)
            {
                ViewBag.SalaIdPendente = salaId;
                ViewBag.NomeSalaPendente = nomeSala;
            }
        }

        return View(viewModelParaPrimeiroItem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Ação POST para processar a requisição de material.
    public async Task<IActionResult> Index(
        DateTime dataRequisicao,
        TimeSpan horaInicio,
        TimeSpan horaFim,
        int[] materialId,
        int[] quantidadeRequisitada,
        int? salaIdPendenteInput)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            TempData["ErrorMessage"] = "Utilizador não autenticado.";
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        if (horaFim <= horaInicio)
        {
            ModelState.AddModelError("HoraFim", "A hora de fim deve ser posterior à hora de início.");
        }
        if (dataRequisicao.Date < DateTime.Today)
        {
            ModelState.AddModelError("DataRequisicao", "A data de requisição não pode ser no passado.");
        }

        bool itensValidosPresentes = false;
        if (materialId == null || quantidadeRequisitada == null || materialId.Length == 0 || materialId.Length != quantidadeRequisitada.Length)
        {
            ModelState.AddModelError(string.Empty, "Nenhum material foi selecionado ou os dados dos itens estão incompletos. Adicione pelo menos um item.");
        }
        else
        {
            for (int i = 0; i < materialId.Length; i++)
            {
                if (materialId[i] <= 0 && quantidadeRequisitada[i] > 0)
                {
                    ModelState.AddModelError($"materialId[{i}]", $"Item {i + 1}: Por favor, selecione um material.");
                }
                else if (materialId[i] > 0 && quantidadeRequisitada[i] <= 0)
                {
                    ModelState.AddModelError($"quantidadeRequisitada[{i}]", $"Item {i + 1}: A quantidade deve ser pelo menos 1.");
                }
                else if (materialId[i] > 0 && quantidadeRequisitada[i] > 0)
                {
                    itensValidosPresentes = true;
                }
            }
            if (!itensValidosPresentes && materialId.Length > 0)
            {
                ModelState.AddModelError(string.Empty, "Preencha os dados dos materiais a requisitar (material e quantidade).");
            }
        }

        var requisicaoValida = new List<RequisicaoMaterial>();
        // Só prosseguir com a lógica de disponibilidade se o ModelState básico for válido e houver itens válidos
        if (ModelState.IsValid && itensValidosPresentes)
        {
            bool todosItensDisponiveis = true;
            for (int i = 0; i < materialId.Length; i++)
            {
                // Ignorar linhas onde o material não foi selecionado ou quantidade é inválida
                if (materialId[i] <= 0 || quantidadeRequisitada[i] <= 0) continue;

                int currentMaterialId = materialId[i];
                int currentQuantidade = quantidadeRequisitada[i];

                var materialInfo = await _context.Materiais.FindAsync(currentMaterialId);
                if (materialInfo == null)
                {
                    ModelState.AddModelError($"materialId[{i}]", $"Item {i + 1}: Material ID {currentMaterialId} não encontrado.");
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

                if (currentQuantidade > disponivelNoPeriodo)
                {
                    ModelState.AddModelError($"quantidadeRequisitada[{i}]",
                        $"Item {i + 1} ({materialInfo.Nome}): Apenas {Math.Max(0, disponivelNoPeriodo)} disponíveis. Pedido: {currentQuantidade}.");
                    todosItensDisponiveis = false;
                }
                else
                {
                    requisicaoValida.Add(new RequisicaoMaterial
                    {
                        UtilizadorId = currentUser.Id,
                        MaterialId = currentMaterialId,
                        QuantidadeRequisitada = currentQuantidade,
                        DataRequisicao = dataRequisicao.Date,
                        HoraInicio = horaInicio,
                        HoraFim = horaFim,
                    });
                }
            }
            if (!todosItensDisponiveis)
            {
                requisicaoValida.Clear(); // Não salvar se algum item não estiver disponível
            }
        }

        if (ModelState.IsValid && requisicaoValida.Any())
        {
            try
            {
                _context.RequisicoesMaterial.AddRange(requisicaoValida);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{requisicaoValida.Count} requisição(ões) de material efetuada(s) com sucesso!";
                return RedirectToAction("Salas", "Alugar");
            }
            catch (DbUpdateException)
            {
                // Para um ambiente de produção, é melhor usar um sistema de logging mais robusto (ex: Serilog, NLog)
                // Console.WriteLine($"Controller: Erro DbUpdateException ao guardar - {ex.ToString()}");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao tentar guardar as requisições.");
            }
        }

        // Se chegou aqui, ou ModelState é inválido ou não há requisições válidas para salvar
        if (!ModelState.IsValid && itensValidosPresentes && !requisicaoValida.Any())
        {
            if (!ModelState.Values.SelectMany(v => v.Errors).Any(e => e.ErrorMessage.Contains("disponíveis")))
            {
                ModelState.AddModelError(string.Empty, "Nenhum dos materiais solicitados estava disponível nas quantidades/horários pedidos.");
            }
        }

        await PopulateViewBagAsync();
        // Recriar o modelo para a view com os dados submetidos para repopular o formulário
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

        return View(modelParaRetorno);
    }
}