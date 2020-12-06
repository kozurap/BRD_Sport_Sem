using System.ComponentModel.DataAnnotations;

namespace BRD_Sport_Sem.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Не указан Email")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Не указано Имя")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Не указана Фамилия")]
        public string Surname { get; set; }
        
        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Compare("Password", ErrorMessage = "Пароль введен неверно")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}