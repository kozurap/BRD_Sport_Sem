﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ProjectArt.MVCPattern.Services
{
    public interface IViewRenderService
    {
        Task<string> RenderAsync(string viewName, object model, Controller controller, bool isMainPage = true);
        IView GetView(ActionContext actionContext, string viewName, bool isMainPage = true);
    }
    
    public class ViewRenderService : IViewRenderService
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
 
        public ViewRenderService(IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }
        
        public async Task<string> RenderAsync(string viewName, object model, Controller controller, bool isMainPage = true)
        {
            var actionContext = new ActionContext(controller.Context, controller.RouteData, new ActionDescriptor());
            var view = GetView(actionContext, viewName, isMainPage);
            await using (var output = new StringWriter())
            {
                controller.ViewData.Model = model;
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    controller.ViewData,
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                await view.RenderAsync(viewContext);

                return output.ToString();
            }
        }

        public IView GetView(ActionContext actionContext, string viewName, bool isMainPage = true)
        {
            var getViewResult = _razorViewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: isMainPage);
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = _razorViewEngine.FindView(actionContext, viewName, isMainPage: isMainPage);
            if (findViewResult.Success)
            {
                return findViewResult.View;
            }

            var searchedLocations = new StringBuilder();
            foreach (var location in getViewResult.SearchedLocations)
                searchedLocations.Append($"{location}\n");
            
            foreach (var location in findViewResult.SearchedLocations)
                searchedLocations.Append($"{location}\n");
            throw new InvalidOperationException(
                $"View '{viewName}' does not exist in directories {searchedLocations}");
        }
    }
}