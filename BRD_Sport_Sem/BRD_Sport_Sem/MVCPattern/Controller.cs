﻿using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProjectArt.MVCPattern.ActionResults;
using ProjectArt.MVCPattern.Services;
using ContentResult = ProjectArt.MVCPattern.ActionResults.ContentResult;
using FileResult = ProjectArt.MVCPattern.ActionResults.FileResult;
using RedirectResult = ProjectArt.MVCPattern.ActionResults.RedirectResult;
using ViewResult = ProjectArt.MVCPattern.ActionResults.ViewResult;

namespace ProjectArt.MVCPattern
{
    public class Controller
    {
        public HttpContext Context;
        public HttpRequest Request;
        public HttpResponse Response;
        public Route RouteInfo;
        public RouteData RouteData;
        public RouteValueDictionary RouteValues;
        public ViewDataDictionary ViewData =
            new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());

        public ModelBindingState ModelBindingState;

        public dynamic _viewBag;

        public dynamic ViewBag
        {
            get
            {
                if (_viewBag == null) _viewBag = new DynamicViewBag(() => ViewData);
                return _viewBag;
            }
        }

        public virtual void Initialize()
        {

        }

        public string Url(string contentPath)
        {
            if (string.IsNullOrEmpty(contentPath))
            {
                return null;
            }
            else if (contentPath[0] == '~')
            {
                var segment = new PathString(contentPath.Substring(1));
                var applicationPath = Context.Request.PathBase;

                return applicationPath.Add(segment).Value;
            }

            return contentPath;
        }
        
        protected RedirectResult Redirect(string destinationUrl, bool isPermanent = false)
            => Context.RequestServices.GetService<IActionResultProvider<RedirectResult>>()
                .GetResult(destinationUrl, isPermanent);
        
        protected JSONResult Json(object obj, int statusCode = 200)
            => Context.RequestServices.GetService<IActionResultProvider<JSONResult>>()
                .GetResult(obj, statusCode);
        
        protected JSONContentResult JsonContent(string json, int statusCode = 200)
            => Context.RequestServices.GetService<IActionResultProvider<JSONContentResult>>()
                .GetResult(json, statusCode);

        protected StatusResult Status(int statusCode)
            => Context.RequestServices.GetService<IActionResultProvider<StatusResult>>()
                .GetResult(statusCode);

        protected StatusResult Ok()
            => Status(200);
        
        protected StatusResult BadRequest()
            => Status(400);
        
        protected StatusResult ServerError()
            => Status(500);

        protected ContentResult Content(string content = null, string contentType = null, int? statusCode = null)
            => Context.RequestServices.GetService<IActionResultProvider<ContentResult>>()
                .GetResult(content, contentType, statusCode);

        protected ContentResult Content(object content = null, string contentType = null, int? statusCode = null)
            => Content(content?.ToString(), contentType, statusCode);

        protected ViewResult View(string viewName, object model)
            => Context.RequestServices.GetService<IActionResultProvider<ViewResult>>()
                .GetResult(viewName, model);

        protected ViewResult View(object model)
            => View("", model);

        protected ViewResult View(string viewName = "")
            => View(viewName, new object());

        protected FileResult File(string path, string contentType = "", int? statusCode = 200)
            => Context.RequestServices.GetService<IActionResultProvider<FileResult>>()
                .GetResult(path, contentType, statusCode);
        
        protected StreamResult Stream(Stream stream, string contentType = "", int? statusCode = 200)
            => Context.RequestServices.GetService<IActionResultProvider<StreamResult>>()
                .GetResult(stream, contentType, statusCode);
    }
}