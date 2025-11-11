using System.ComponentModel.DataAnnotations;

namespace ValiModern.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password), MinLength(6, ErrorMessage = "Password ít nhất 6 ký tự.")]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Password không khớp.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}