﻿using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace ProjectArt.MVCPattern.ActionResults
{
    public class StatusResult : IActionResult
    {
        public int StatusCode { get; set; } = 200;

        public StatusResult(int statusCode)
        {
            StatusCode = statusCode;
        }

        public async Task ExecuteResult(Controller controller)
        {
            controller.Response.StatusCode = StatusCode;
            await controller.Response.CompleteAsync();
        }
    }
}