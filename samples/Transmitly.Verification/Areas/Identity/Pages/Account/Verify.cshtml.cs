using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using VerifyV2Quickstart.Models;
using VerifyV2Quickstart.Services;

namespace VerifyV2Quickstart.Areas.Identity.Pages.Account
{
    [Authorize]
    public class VerifyModel: PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVerification _verification;
        private readonly ILogger<VerifyModel> _logger;
        
        public VerifyModel(
            UserManager<ApplicationUser> userManager,
            IVerification verification,
            ILogger<VerifyModel> logger)
        {
            _userManager = userManager;
            _verification = verification;
            _logger = logger;
        }
        
        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Verification Code")]
            public string Code { get; set; }
        }
        
        public ActionResult OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<ActionResult> OnPostAsync(string returnUrl = null)
        {
            ApplicationUser user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return LocalRedirect(Url.Content($"~/Identity/Account/Login/?returnUrl={returnUrl}"));
            }

            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var result = await _verification.CheckVerificationAsync(user.PhoneNumber, Input.Code);
                if (result.IsValid)
                {
                    user.Verified = true;
                    await _userManager.UpdateAsync(user);

                    _logger.Log(LogLevel.Information, $"User verified: {user.UserName}");

                    return LocalRedirect(Url.Content(returnUrl));
                }
                
                foreach (var error in result.Errors)
                {
                    _logger.Log(LogLevel.Information, $"Verification Failed: {error}");

                    ModelState.AddModelError(string.Empty, error);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}