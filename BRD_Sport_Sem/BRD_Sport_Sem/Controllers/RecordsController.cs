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
using System.Text.Json;

namespace BRD_Sport_Sem.Controllers
{
    [Controller("Records")]
    public class RecordsController:Controller
    {
        private DataGateORM _db;

        [Action("~/Records", Method = MethodType.GET)]
        public IActionResult Records()
        {
            return View("Records");
        }

        public RecordsController(DataGateORM db)
        {
            _db = db;
        }
        [Action("GetList", Method = MethodType.GET)]
        public IActionResult GetList()
        {
            return Json(_db.Get<Record>().ToList().Values.ToList()); //return List<Tournament>
        }

        [Action("",Method = MethodType.POST)]
        public IActionResult Post(Record record)
        {
            /*if (json == null)
                return ServerError();
            Record record = JsonSerializer.Deserialize<Record>(json);*/
            try
            {
                Regex reg = new Regex(@"[A-ZА-Я]?[a-zа-я]+");
                if (reg.IsMatch(record.Author) == true)
                    _db.Insert<Record>(record);
                else
                    return ServerError();
                return View("Records");
            }
            catch
            {
                Console.WriteLine("Invalid input");
                return ServerError();
            }
        }
        [Action("SearchByAuthor",Method = MethodType.GET)]
        public IActionResult Search(string author)
        {
            return Json(_db.Get<Record>().ToList().Values.ToList().Where(u=>u.Author == author));
        }
    }
}