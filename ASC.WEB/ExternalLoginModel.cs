using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ASC.WEB.Pages.Account // ✅ đúng với @model bạn đang dùng
{
    public class ExternalLoginModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email is required.")]
            [EmailAddress(ErrorMessage = "Invalid email address.")]
            public string Email { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ProviderDisplayName = "Google"; // 👈 Hoặc lấy từ TempData / ViewData nếu cần
        }

        public IActionResult OnPostConfirmation(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Thêm logic đăng ký người dùng mới (nếu cần)
            TempData["SuccessMessage"] = "Registered with external provider successfully!";
            return LocalRedirect(ReturnUrl);
        }
    }
}
