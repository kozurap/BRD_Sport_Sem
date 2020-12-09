﻿using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using ProjectArt.MVCPattern.Services;

namespace ProjectArt.MVCPattern.ActionResults
{
    public class JSONResult : IActionResult
    {
        public object Content { get; set; } = null;
        public int? StatusCode { get; set; } = null;

        public IActionResultProvider<JSONContentResult> JsonContentResultProvider;

        public JSONResult(IActionResultProvider<JSONContentResult> jsonContentResultProvider, object content = null, int? statusCode = null)
        {
            JsonContentResultProvider = jsonContentResultProvider;
            Content = content;
            StatusCode = statusCode;
        }

        public async Task ExecuteResult(Controller controller)
        {
            var content = JsonConvert.SerializeObject(Content);
            await JsonContentResultProvider.GetResult(content, StatusCode)
                .ExecuteResult(controller);
        }
    }
}