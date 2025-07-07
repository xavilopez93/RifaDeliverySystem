using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RifaDeliverySystem.Web.Data;

namespace RifaDeliverySystem.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
                return RedirectToPage("/Index");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"No se pudo cargar al usuario con ID '{userId}'.");

            var result = await _userManager.ConfirmEmailAsync(user, code);
            StatusMessage = result.Succeeded
                ? "�Gracias por confirmar tu correo!"
                : "Error al confirmar tu correo.";
            return Page();
        }
    }
}
