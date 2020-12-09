using DataGate.Core.Attributes;

namespace BRD_Sport_Sem.Models
{
    public class User
    {    
        [Id]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Password { get; set; }
        public byte[] ProfileImage;
    }
}