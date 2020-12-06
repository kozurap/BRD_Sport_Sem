using System;
using System.Collections.Generic;
using BRD_Sport_Sem.Models;
using ProjectArt.MVCPattern.Attributes;
using Controller = ProjectArt.MVCPattern.Controller;
using IActionResult = ProjectArt.MVCPattern.IActionResult;

namespace BRD_Sport_Sem.Controllers
{
    [ProjectArt.MVCPattern.Attributes.Controller("Tournament")]
    public class TournamentsController : Controller
    {
        List<Tournament> _tournaments = new List<Tournament>();
        
        [Action("",Method = MethodType.GET)]
        public IActionResult Get()
        {
            return View();
        }
        
        [Action("",Method = MethodType.GET)]
        public List<Tournament> GetList() => _tournaments;

        [Action("",Method = MethodType.POST)]
        public List<Tournament> Post(Tournament tournament)
        {
            try
            {
                _tournaments.Add(tournament);
                return _tournaments;
            }
            catch
            {
                Console.WriteLine("Invalid input");
                return null;
            }
        }
    }
}