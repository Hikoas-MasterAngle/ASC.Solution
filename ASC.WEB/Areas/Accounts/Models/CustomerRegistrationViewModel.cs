using System.ComponentModel.DataAnnotations;

namespace ASC.WEB.Areas.Accounts.Models
{
    public class CustomerRegistrationViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string UserName { get; set; }

        // Chỉ cần 1 khai báo duy nhất
        public bool IsEdit { get; set; }

        public bool IsActive { get; set; }
    }
}
