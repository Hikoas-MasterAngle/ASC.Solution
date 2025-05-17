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
            // Lấy Email của người dùng hiện tại
            var userEmail = HttpContext.User.GetCurrentUserDetails().Email;
            var user = await _userManager.FindByEmailAsync(userEmail);

            // Kiểm tra User có tồn tại không
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email không tồn tại.");
                return Page();
            }

            // Tạo Reset Token
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Tạo URL Reset Password
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { userId = user.Id, code = encodedCode },
                protocol: Request.Scheme);

            // Debug: Kiểm tra Token và URL có đúng không
            Console.WriteLine("Generated Reset Token: " + code);
            Console.WriteLine("Encoded Reset Token: " + encodedCode);
            Console.WriteLine("Reset Password Email Sent to: " + userEmail);
            Console.WriteLine("Reset Password Link: " + callbackUrl);

            // Gửi Email Reset Password
            await _emailSender.SendEmailAsync(userEmail, "Reset Password",
                $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

            return LocalRedirect("/Identity/Account/ResetPasswordEmailConfirmation");
        }
    }
}