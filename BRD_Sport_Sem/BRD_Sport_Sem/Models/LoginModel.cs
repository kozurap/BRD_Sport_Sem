using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace BRD_Sport_Sem.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан Email")]
        public string Email { get; set; }
         
        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        public string GetPasswordHash()
        {
            var hashBuilder = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                var result = hash.ComputeHash(Encoding.UTF8.GetBytes(Password));
                foreach (var b in result)
                    hashBuilder.Append(b.ToString("x2"));
            }

            return hashBuilder.ToString();
        }
    }
}