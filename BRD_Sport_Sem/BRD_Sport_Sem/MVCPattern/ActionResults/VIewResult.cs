﻿using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using ProjectArt.MVCPattern.Services;

namespace ProjectArt.MVCPattern.ActionResults
{
    public class ViewResult : IActionResult
    {
        private string _defaultContentType = "text/html; charset=utf-8";

        public ViewResult(string viewName, object model, IViewRenderService viewRenderService)
        {
            ViewRenderService = viewRenderService;
            ViewName = viewName;
            Model = model;
        }

        public int? StatusCode { get; set; } = null;
        public object Model { get; }
        public string ViewName { get; private set; }
        public IViewRenderService ViewRenderService { get; private set; }

        public async Task ExecuteResult(Controller controller)
        {
            var response = controller.Context.Response;

            if(string.IsNullOrEmpty(ViewName))
                ViewName = (string)controller.RouteValues["action"];
            
            if(ViewRenderService == null) 
                ViewRenderService = controller.Context.RequestServices.GetService<IViewRenderService>();
            
            var content = await ViewRenderService.RenderAsync(ViewName, Model, controller);
            
            var encoding = MediaType.GetEncoding(_defaultContentType);
            response.ContentType = _defaultContentType;
            if(StatusCode != null) response.StatusCode = StatusCode.Value;
            if (content != null)
            {
                response.ContentLength = encoding.GetByteCount(content);
                await response.WriteAsync(content, encoding);
            }
            
            await response.CompleteAsync();
        }
    }
}