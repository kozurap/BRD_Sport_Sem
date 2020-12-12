namespace BRD_Sport_Sem.Models.ViewModel
{
    public class ProfileViewModel
    {
        public int UserId;
        public string Name;
        public string Surname;
        public string Email;

        public static ProfileViewModel GetFromUserModel(User user)
        {
            var model = new ProfileViewModel()
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname
            };
            return model;
        }
    }
}