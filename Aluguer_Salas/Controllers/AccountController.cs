using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity; // Certifique-se que tem este using
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class AccountController : Controller
{
    // Mude de ApplicationUser para IdentityUser aqui
    private readonly SignInManager<IdentityUser> _signInManager;

    // E aqui também
    public AccountController(SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}