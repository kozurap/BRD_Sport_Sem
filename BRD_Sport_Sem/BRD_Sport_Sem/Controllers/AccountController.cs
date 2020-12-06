using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BRD_Sport_Sem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using ProjectArt.MVCPattern.Attributes;
using Controller = ProjectArt.MVCPattern.Controller;
using IActionResult = ProjectArt.MVCPattern.IActionResult;

namespace BRD_Sport_Sem.Controllers
{
    [ProjectArt.MVCPattern.Attributes.Controller("Account")]
    public class AccountController : Controller
    {
        List<User> users = new List<User>();
        [Action("Login", Method = MethodType.GET)]
        public IActionResult Login()
        {
            return View("Login");
        }

        [Action("Login", Method = MethodType.POST)]
        public IActionResult Login(LoginModel model)
        {
            if (ModelBindingState.IsAllSet)
            {
                User user = users.FirstOrDefault(u => u.Email == model.Email);
                if (user != null)
                {
                    Authenticate(model.Email);
                    return View("Profile");
                }
            }

            return View(model);
        }
        public  IActionResult Register(RegisterModel model)
        {
            User user = users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                users.Add(new User()
                    {Email = model.Email, Password = model.Password, Name = model.Name, Surname = model.Surname});
                Authenticate(model.Email);
                return View("Profile");
            }
            return View(model);
        }

        private async Task Authenticate(string modelEmail)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, modelEmail)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", 
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
           }

        [Action("Register", Method = MethodType.GET)]
        public IActionResult Register()
        {
            return View("Register");
        }
       }
}