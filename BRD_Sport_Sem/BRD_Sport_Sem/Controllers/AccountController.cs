using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using BRD_Sport_Sem.Models;
using BRD_Sport_Sem.Models.ViewModel;
using DataGate.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using ProjectArt.MVCPattern.Attributes;
using Controller = ProjectArt.MVCPattern.Controller;
using IActionResult = ProjectArt.MVCPattern.IActionResult;

namespace BRD_Sport_Sem.Controllers
{
    [Controller("Account")]
    public class AccountController : Controller
    {
        private DataGateORM _db;

        public AccountController(DataGateORM db)
        {
            _db = db;
        }

        [Action("Login", Method = MethodType.GET)]
        public async Task<IActionResult> Login()
        {
            var token = Context.Session.GetString("AuthToken");
            if (token != null)
                 return Redirect(Url("~/Account/Profile"));
            if (Context.Request.Cookies.ContainsKey("name") && Context.Request.Cookies.ContainsKey("password"))
            {
                LoginModel model = new LoginModel()
                {
                    Email = Context.Request.Cookies["name"],
                    Password = Context.Request.Cookies["password"]
                };
                User user = _db.Get<User>().ToList().Values.FirstOrDefault(u => u.Email == model.Email);
                if (user != null)
                {
                    if (model.Password == user.Password)
                    {
                        await Authenticate(model.Email);
                        return Redirect(Url("~/Account/Profile"));
                    }
                }
            }
            return View("Login");
        }

        [Action("Login", Method = MethodType.POST)]
        public async Task<IActionResult> Login(LoginModel model, string loginkeeping)
        {
            if (ModelBindingState.IsSet("model"))
            {
                List<User> users = new List<User>();
                var query = _db.Get<User>();
                foreach (var e in query.ToList())
                {
                    users.Add(e.Value);
                }

                User user = users.FirstOrDefault(u => u.Email == model.Email);
                if (user != null)
                {
                    var p = model.GetPasswordHash();
                    if (model.GetPasswordHash() == user.Password)
                    {
                        await Authenticate(model.Email);
                        if (loginkeeping == "loginkeeping")
                        {
                            await RememberMe(model);
                        }
                        return Redirect(Url("~/Account/Profile"));
                    }
                }
            }

            return Redirect(Url("~/Account/Login"));
        }

        private async Task RememberMe(LoginModel model)
        {
            Context.Response.Cookies.Append("name", model.Email);
            Context.Response.Cookies.Append("password", model.GetPasswordHash());
        }

        private async Task Authenticate(string modelEmail)
        {
            Context.Session.SetString("AuthToken", modelEmail);
        }

        [Action("Register", Method = MethodType.GET)]
        public IActionResult Register()
        {
            var token = Context.Session.GetString("AuthToken");
            if (token != null)
                return Redirect(Url("~/Account/Profile"));
            return View("Register");
        }

        [Action("Register", Method = MethodType.POST)]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelBindingState.IsAllSet)
            {
                List<User> users = new List<User>();
                var query = _db.Get<User>();
                foreach (var e in query.ToList())
                {
                    users.Add(e.Value);
                }

                User user = users.FirstOrDefault(u => u.Email == model.Email);
                if (user == null)
                {
                    User reg = new User()
                        {Email = model.Email, Name = model.Name, Surname = model.Surname, Password = model.GetPasswordHash()};
                    _db.Insert<User>(reg);
                    await Authenticate(model.Email);
                    return Redirect(Url("~/Account/Profile"));
                }
            }

            return Redirect(Url("~/Account/Login"));
        }

        [Action("Profile", Method = MethodType.GET)]
        public IActionResult Profile()
        {
            string token = Context.Session.GetString("AuthToken");
            if (token == null)
                Redirect(Url("~/Account/Login"));
            var query = _db.Get<User>();
            User user = query.ToList().Values.FirstOrDefault(u => u.Email == token);
            if (user != null)
            {
                return View("Profile",ProfileViewModel.GetFromUserModel(user));
            }

            return Redirect(Url("~/Account/Login"));
        }
        [Action("Profile/{id}/ProfileEdit", Method = MethodType.GET)]
        public IActionResult ProfileEdit()
        {
            string token = Context.Session.GetString("AuthToken");
            if (token == null)
                Redirect(Url("~/Account/Login"));
            var query = _db.Get<User>();
            User user = query.ToList().Values.FirstOrDefault(u => u.Email == token);
            if (user != null)
            {
                return View("ProfileEdit",ProfileViewModel.GetFromUserModel(user));
            }

            return Redirect(Url("~/Account/Login"));
        }

        [Action("Profile/{id}/ProfileEdit", Method = MethodType.POST)]
        public async Task<IActionResult> ProfileEdit(IFormFile file)
        {
            string token = Context.Session.GetString("AuthToken");
            if (token == null)
                Redirect(Url("~/Account/Login"));
            var users = _db.Get<User>()
                .Where(u => u.Email == token).ToList();

            if (users.Count == 0)
                return Status(404);

            if (users.Count > 1)
                return ServerError();

            var userObj = users[0];
            var user = userObj.Value;

            if(user == null)
                Redirect(Url("~/Account/Login"));
            if (file != null)
            {
                var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                user.ProfileImage = ms.ToArray();
            }
            userObj.Update();

            return Redirect(Url("~/Account/Profile"));
        }

        [Action("/{id}/Image")] //Account/{id}/Image
        public IActionResult GetUserImage(int id)
        {
            var users = _db.Get<User>()
                .Where(u => u.Id == id).ToList();
            if (users.Count == 0)
                return Status(404);

            if (users.Count > 1)
                return ServerError();

            var user = users[0].Value;
            if(user.ProfileImage == null)
                return File("/wwwroot/DefaultImage.jpg");

            return Stream(new MemoryStream(user.ProfileImage));
        }

        [Action("~/Logout")]
        public IActionResult Logout()
        {
            Context.Session.Clear();
            return Redirect(Url("~/"));
        }
    }
}