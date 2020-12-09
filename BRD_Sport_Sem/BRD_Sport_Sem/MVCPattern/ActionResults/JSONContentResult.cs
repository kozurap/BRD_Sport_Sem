﻿using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using ProjectArt.MVCPattern.Services;

namespace ProjectArt.MVCPattern.ActionResults
{
    public class JSONContentResult : IActionResult
    {
        public string Content { get; set; } = null;
        public int? StatusCode { get; set; } = null;

        public IActionResultProvider<ContentResult> ContentResultProvider;

        public JSONContentResult(IActionResultProvider<ContentResult> contentResultProvider, string content = null, int? statusCode = null)
        {
            ContentResultProvider = contentResultProvider;
            Content = content;
            StatusCode = statusCode;
        }

        public async Task ExecuteResult(Controller controller)
        {
            await ContentResultProvider.GetResult(Content, "application/json", StatusCode)
                .ExecuteResult(controller);
        }
    }
}