using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public ResetPasswordModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nova senha")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nova senha")]
        [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public IActionResult OnGet(string code = null, string email = null)
    {
        if (code == null || email == null)
        {
            return RedirectToPage("./ForgotPassword");
        }
        Input = new InputModel
        {
            Code = code,
            Email = email
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            // Não revelar que o usuário não existe
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
        if (result.Succeeded)
        {
            return RedirectToPage("./ResetPasswordConfirmation");
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return Page();
    }
}
