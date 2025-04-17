using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ASC.Web.Areas.Identity.Pages.Account
{
    public class ExternalLoginConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<ExternalLoginConfirmationModel> _logger;


        public ExternalLoginConfirmationModel(SignInManager<IdentityUser> signInManager,
                                              ILogger<ExternalLoginConfirmationModel> logger,
                                              UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./ExternalLoginFailure");
            }

            var user = new IdentityUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                // Add Claims
                var resultUserClaim = await _userManager.AddClaimAsync(user,
                    new Claim(ClaimTypes.Email, user.Email));
                var resultActiveClaim = await _userManager.AddClaimAsync(user,
                    new Claim("IsActive", "true"));

                if (!resultUserClaim.Succeeded || !resultActiveClaim.Succeeded)
                {
                    foreach (var error in resultUserClaim.Errors.Concat(resultActiveClaim.Errors))
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                // Gán role Engineer n?u có enum Roles.Engineer
                var roleResult = await _userManager.AddToRoleAsync(user, "Engineer");
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                await _userManager.AddLoginAsync(user, info);

                await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return Page();
        }
    }
}
