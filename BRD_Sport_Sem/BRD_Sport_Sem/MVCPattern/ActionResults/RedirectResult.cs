﻿using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace ProjectArt.MVCPattern.ActionResults
{
    public class RedirectResult : IActionResult
    {
        private int _statusCodeTemp = StatusCodes.Status302Found;
        private int _statusCodePermanent = StatusCodes.Status301MovedPermanently;

        private string _destUrl;
        private bool _isPermanent;

        public RedirectResult(string destUrl, bool isPermanent)
        {
            _destUrl = destUrl;
            _isPermanent = isPermanent;
        }

        public async Task ExecuteResult(Controller controller)
        {
            controller.Response.StatusCode = 
                _isPermanent ? _statusCodePermanent
                : _statusCodeTemp;
            
            controller.Response.Headers[HeaderNames.Location] = _destUrl;
            
            await controller.Response.CompleteAsync();
        }
    }
}