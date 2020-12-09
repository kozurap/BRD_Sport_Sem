using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BRD_Sport_Sem.Models;
using DataGate.Core;
using ProjectArt.MVCPattern.Attributes;
using Controller = ProjectArt.MVCPattern.Controller;
using IActionResult = ProjectArt.MVCPattern.IActionResult;

namespace BRD_Sport_Sem.Controllers
{
    [ProjectArt.MVCPattern.Attributes.Controller("Tournament")]
    public class TournamentsController : Controller
    {
        private DataGateORM _db;

        public TournamentsController(DataGateORM db)
        {
            _db = db;
        }
        
        [Action("",Method = MethodType.GET)]
        public IActionResult Get()
        {
            return View("Tournaments");
        }

        [Action("", Method = MethodType.GET)]
        public IActionResult GetList()
        {
            return Json(_db.Get<Tournament>().ToList().Values.ToList()); //return List<Tournament>
        }

        [Action("",Method = MethodType.POST)]
        public IActionResult Post(Tournament tournament)
        {
            try
            {
                Regex reg = new Regex(@"\d\d\.\d\d\.\d\d");
                if (!(reg.IsMatch(tournament.Date)))
                    return ServerError();
                _db.Insert<Tournament>(tournament);
                return Json(_db.Get<Tournament>().ToList().Values.ToList()); //return List<Tournament>
            }
            catch
            {
                Console.WriteLine("Invalid input");
                return ServerError();
            }
        }
        [Action("",Method = MethodType.GET)]
        public IActionResult Search(string name)
        {
            return Json(_db.Get<Tournament>().ToList().Values.Where(u => u.Name == name).ToList());
            //return List<Tournament>.Where(u=>u.Name == name)
        }
    }
}