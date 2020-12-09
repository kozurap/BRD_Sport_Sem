using System;
using System.Collections.Generic;
using System.Linq;
using BRD_Sport_Sem.Models;
using DataGate.Core;
using Microsoft.AspNetCore.Http;
using Npgsql;
using ProjectArt.MVCPattern;
using ProjectArt.MVCPattern.Attributes;

namespace BRD_Sport_Sem.Controllers
{
    [Controller("Forum")]
    public class ForumController:Controller
    {
        private DataGateORM _db;
        public ForumController(DataGateORM db)
        {
            _db = db;
        }

        [Action("~/Forum", Method = MethodType.GET)]
        public IActionResult Forum()
        {
            return View("Forum");
        }
        [Action("~/Forum",Method = MethodType.GET)]
        public IActionResult GetTopics()
        {
            var query = _db.Get<Forum>();
            List<string> res = query.ToList().Values.Select(u => u.Name).ToList();
            return Json(res);
        }

        [Action("/{id}", Method = MethodType.GET)] //Forum.Id
        public IActionResult ForumTopic(int id)
        {
            var Topics = _db.Get<Forum>()
                .Where(u => u.Id == id).ToList().Values.Select(u=>u.Content).ToList();
            if (Topics.Count == 0)
                return Status(404);
            if (Topics.Count > 1)
                return ServerError();
            var topic = Topics[0]; // <Author,Content>
            return Json(topic);
        }

        [Action("/{id}", Method = MethodType.POST)]
        public IActionResult ForumTopic(int id,string message)
        {
            string token = Context.Session.GetString("AuthToken");
            if (token == null)
                Redirect(Url("~/Account/Login"));
            var uquery = _db.Get<User>();
            List<User> users = uquery.Where(u => u.Email == token).ToList().Values.ToList();
            if (users.Count == 0)
                return Status(404);

            if (users.Count > 1)
                return ServerError();
            var userName = users[0].Name + " " + users[0].Surname;
            string connectionString =
                "Host=localhost;Port=5432;Username=postgres;Password=1q2w3e4r5t;Database=postgres;";
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            string sqlcommand = "Update FORUM set Content = Content || " + userName + " => " + message;
            var command = new NpgsqlCommand(sqlcommand, connection);
            command.ExecuteNonQuery();
            connection.Close();
            return Json(userName + " " + message);
        }

        [Action("/Create", Method = MethodType.GET)]
        public IActionResult CreateTopic()
        {
            return View("CreateTopic");
        }
        [Action("/Create", Method = MethodType.POST)]
        public IActionResult CreateTopic(string name, string content)
        {
            string token = Context.Session.GetString("AuthToken");
            if (token == null)
                Redirect(Url("~/Account/Login"));
            var uquery = _db.Get<User>();
            List<User> users = uquery.Where(u => u.Email == token).ToList().Values.ToList();
            if (users.Count == 0)
                return Status(404);

            if (users.Count > 1)
                return ServerError();
            var userName = users[0].Name + " " + users[0].Surname;
            var dict = new Dictionary<string,string>();
            dict.Add(userName,content);
            var forum = new Forum()
            {
                Content = dict,
                Name = name
            };
            _db.Insert<Forum>(forum);
            return Redirect(Url("~/Forum/" + forum.Id));
        }
    }
}