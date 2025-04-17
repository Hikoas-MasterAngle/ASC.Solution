using ASC.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ASC.WEB.Services;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace ASC.WEB.Areas.Identity.Pages.Account
{
    public class InitiateResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public InitiateResetPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // L?y Email c?a ng??i dùng hi?n t?i
            var userEmail = HttpContext.User.GetCurrentUserDetails().Email;
            var user = await _userManager.FindByEmailAsync(userEmail);

            // Ki?m tra User có t?n t?i không
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email không t?n t?i.");
                return Page();
            }

            // T?o Reset Token
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // T?o URL Reset Password
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { userId = user.Id, code = encodedCode },
                protocol: Request.Scheme);

            // Debug: Ki?m tra Token và URL có ?úng không
            Console.WriteLine("Generated Reset Token: " + code);
            Console.WriteLine("Encoded Reset Token: " + encodedCode);
            Console.WriteLine("Reset Password Email Sent to: " + userEmail);
            Console.WriteLine("Reset Password Link: " + callbackUrl);

            // G?i Email Reset Password
            await _emailSender.SendEmailAsync(userEmail, "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

            return LocalRedirect("/Identity/Account/ResetPasswordEmailConfirmation");
        }
    }
}