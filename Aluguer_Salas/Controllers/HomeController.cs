using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Aluguer_Salas.Models;

namespace Aluguer_Salas.Controllers;

public class HomeController : Controller
{
    // O controlador HomeController é responsável por gerenciar as ações relacionadas à página inicial e à privacidade.
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // Esta ação serve para tratar os erros e mostrar a página de erro.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
