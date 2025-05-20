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
        ViewBag.ListaMateriaisDisponiveis = new SelectList(materiais, "Id", "Nome");
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
            // HoraInicio e HoraFim podem ser inicializadas aqui se desejar
            // HoraInicio = TimeSpan.Zero, 
            // HoraFim = TimeSpan.Zero 
        };

        if (TempData.TryGetValue("SalaIdPendente", out object salaIdObj) && TempData.TryGetValue("NomeSalaPendente", out object nomeSalaObj))
        {
            if (salaIdObj is int salaId && nomeSalaObj is string nomeSala)
            {
                ViewBag.SalaIdPendente = salaId;
                ViewBag.NomeSalaPendente = nomeSala;
            }
        }
        // ViewData["Title"] é definido na view.
        return View(viewModelParaPrimeiroItem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        DateTime dataRequisicao,     // Recebido de asp-for="DataRequisicao"
        TimeSpan horaInicio,         // Recebido de asp-for="HoraInicio"
        TimeSpan horaFim,            // Recebido de asp-for="HoraFim"
        int[] materialId,            // Recebido de name="materialId[index]"
        int[] quantidadeRequisitada, // Recebido de name="quantidadeRequisitada[index]"
        int? salaIdPendenteInput)
    {
        Console.WriteLine("POST: RequisitarMaterial/Index recebido.");
        Console.WriteLine($"Data: {dataRequisicao}, Inicio: {horaInicio}, Fim: {horaFim}");
        if (materialId != null) Console.WriteLine($"Materiais Count: {materialId.Length}");
        if (quantidadeRequisitada != null) Console.WriteLine($"Quantidades Count: {quantidadeRequisitada.Length}");


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
                if (materialId[i] <= 0 && quantidadeRequisitada[i] > 0) // Apenas erro se quantidade foi preenchida mas material não
                {
                    // Chave do erro corresponde ao campo do formulário para melhor feedback ao lado do campo (se houver asp-validation-for)
                    // ou para clareza no sumário.
                    ModelState.AddModelError($"materialId[{i}]", $"Item {i + 1}: Por favor, selecione um material.");
                }
                else if (materialId[i] > 0 && quantidadeRequisitada[i] <= 0)
                {
                    ModelState.AddModelError($"quantidadeRequisitada[{i}]", $"Item {i + 1}: A quantidade deve ser pelo menos 1.");
                }
                else if (materialId[i] > 0 && quantidadeRequisitada[i] > 0)
                {
                    itensValidosPresentes = true; // Pelo menos um item completo foi enviado
                }
            }
            if (!itensValidosPresentes && materialId.Length > 0) // Se existem linhas mas nenhuma é válida
            {
                ModelState.AddModelError(string.Empty, "Preencha os dados dos materiais a requisitar (material e quantidade).");
            }
        }


        var requisicoesParaSalvar = new List<RequisicaoMaterial>();
        // Só prosseguir com a lógica de disponibilidade se o ModelState básico for válido e houver itens válidos
        if (ModelState.IsValid && itensValidosPresentes)
        {
            bool todosItensDisponiveis = true;
            for (int i = 0; i < materialId.Length; i++)
            {
                // Ignorar linhas onde o material não foi selecionado ou quantidade é inválida,
                // pois já foram tratadas ou são linhas "vazias" que o utilizador não preencheu.
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
                    ModelState.AddModelError($"quantidadeRequisitada[{i}]", // Ou string.Empty
                        $"Item {i + 1} ({materialInfo.Nome}): Apenas {Math.Max(0, disponivelNoPeriodo)} disponíveis. Pedido: {currentQuantidade}.");
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
                        HoraFim = horaFim,
                        // SalaId = salaIdPendenteInput // Descomente e ajuste se sua entidade RequisicaoMaterial tiver SalaId
                    });
                }
            }
            if (!todosItensDisponiveis)
            {
                requisicoesParaSalvar.Clear(); // Não salvar se algum item não estiver disponível
            }
        }

        if (ModelState.IsValid && requisicoesParaSalvar.Any())
        {
            try
            {
                _context.RequisicoesMaterial.AddRange(requisicoesParaSalvar);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{requisicoesParaSalvar.Count} requisição(ões) de material efetuada(s) com sucesso!";
                return RedirectToAction("Salas", "Alugar");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Controller: Erro DbUpdateException ao guardar - {ex.ToString()}");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao tentar guardar as requisições.");
            }
        }

        // Se chegou aqui, ou ModelState é inválido ou não há requisições válidas para salvar
        if (ModelState.ErrorCount > 0)
        {
            Console.WriteLine("Controller: ModelState inválido. Requisições não foram guardadas.");
            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                foreach (var error in modelStateVal.Errors)
                {
                    Console.WriteLine($"Controller - Erro de Validação: Chave='{modelStateKey}', Erro='{error.ErrorMessage}'");
                }
            }
        }
        else if (itensValidosPresentes && !requisicoesParaSalvar.Any())
        {
            // Todos os itens eram válidos, mas nenhum estava disponível/passou nas verificações de disponibilidade
            // A mensagem de erro específica já deve ter sido adicionada ao ModelState
            // Poderia adicionar uma mensagem genérica aqui se nenhuma específica foi adicionada.
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
            // Não podemos facilmente repopular os itens individuais aqui sem um ViewModel mais complexo
            // A view irá renderizar a primeira linha vazia (ou com os defaults)
            // e o JavaScript irá adicionar novas linhas vazias.
            // Os valores dos itens que causaram erro não serão preservados na UI desta forma simples.
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