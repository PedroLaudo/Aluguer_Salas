using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Aluguer_Salas.Models;

namespace Aluguer_Salas.Controllers;

public class HomeController : Controller
{
    // O controlador HomeController � respons�vel por gerenciar as a��es relacionadas � p�gina inicial e � privacidade.
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

    // Esta a��o serve para tratar os erros e mostrar a p�gina de erro.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
