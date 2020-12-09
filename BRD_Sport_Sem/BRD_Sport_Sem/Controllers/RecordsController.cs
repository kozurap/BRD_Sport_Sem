using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BRD_Sport_Sem.Models;
using DataGate.Core;
using Microsoft.AspNetCore.Http;
using ProjectArt.MVCPattern.Attributes;
using Controller = ProjectArt.MVCPattern.Controller;
using IActionResult = ProjectArt.MVCPattern.IActionResult;

namespace BRD_Sport_Sem.Controllers
{
    [Controller("Records")]
    public class RecordsController:Controller
    {
        private DataGateORM _db;

        public RecordsController(DataGateORM db)
        {
            _db = db;
        }
        [Action("", Method = MethodType.GET)]
        public IActionResult GetList()
        {
            return Json(_db.Get<Record>().ToList().Values.ToList()); //return List<Tournament>
        }

        [Action("",Method = MethodType.POST)]
        public IActionResult Post(Record record)
        {
            try
            {
                Regex reg = new Regex(@"[A-Z]?[А-Я]?[a-z]+[а-я]+");
                if (!(reg.IsMatch(record.Author)))
                    return ServerError();
                _db.Insert<Record>(record);
                return Json(_db.Get<Record>().ToList().Values.ToList()); //return List<Tournament>
            }
            catch
            {
                Console.WriteLine("Invalid input");
                return ServerError();
            }
        }
        [Action("",Method = MethodType.GET)]
        public IActionResult Search(string author)
        {
            return Json(_db.Get<Record>().ToList().Values.ToList().Where(u=>u.Author == author));
        }
    }
}