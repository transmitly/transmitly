using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using VerifyV2Quickstart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using VerifyV2Quickstart.Services;

namespace VerifyV2Quickstart.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IVerification _verificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IVerification verificationService,
                ILogger<RegisterModel> logger)
        {
            _verificationService = verificationService;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            [Required]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required]
            [RegularExpression("\\+\\d+", ErrorMessage = "Write the phone number in the international format without spaces.")]
            public string FullPhoneNumber { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required]
            [Display(Name = "Channel")]
            public string Channel { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.UserName, PhoneNumber = Input.FullPhoneNumber, Verified = false};
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var verification =
                        await _verificationService.StartVerificationAsync(user.PhoneNumber, Input.Channel);

                    if (verification.IsValid)
                    {
                        HttpContext.Session.SetString("_UserId", user.Id);
                        await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, false, lockoutOnFailure: true);
                        return LocalRedirect(Url.Content($"~/Identity/Account/Verify/?returnUrl={returnUrl}"));
                    }

                    await _userManager.DeleteAsync(user);

                    foreach (var error in verification.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
