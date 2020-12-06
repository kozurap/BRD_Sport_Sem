using System;
using System.Collections.Generic;
using BRD_Sport_Sem.Models;
using ProjectArt.MVCPattern.Attributes;
using Controller = ProjectArt.MVCPattern.Controller;
using IActionResult = ProjectArt.MVCPattern.IActionResult;

namespace BRD_Sport_Sem.Controllers
{
    [Controller("Records")]
    public class RecordsController:Controller
    {
        List<Record> _records = new List<Record>();
        [Action("",Method = MethodType.GET)]
        public IActionResult Get()
        {
            return View();
        }
        [Action("",Method = MethodType.GET)]
        public List<Record> GetList() => _records;

        [Action("",Method = MethodType.POST)]
        public List<Record> Post(Record record)
        {
            try
            {
                _records.Add(record);
                return _records;
            }
            catch
            {
                Console.WriteLine("Invalid input");
                return null;
            }
        }
    }
}