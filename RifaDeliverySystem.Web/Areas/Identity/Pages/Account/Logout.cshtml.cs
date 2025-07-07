using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RifaDeliverySystem.Web.Data;

namespace RifaDeliverySystem.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager,
                           ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public void OnGet()
        {
            // sólo muestra la página de confirmación
        }

        public async Task OnPostAsync(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión.");

            if (returnUrl != null)
            {
                Response.Redirect(returnUrl);
            }
            else
            {
                Response.Redirect("~/");
            }
        }
    }
}
