using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProjectArt.MVCPattern.ActionResults;
using ProjectArt.MVCPattern.Services;

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

        protected ContentResult Content(string content = null, string contentType = null, int? statusCode = null)
            => Context.RequestServices.GetService<IActionResultProvider<ContentResult>>()
                .GetResult(content, contentType, statusCode);

        protected ContentResult Content(object content = null, string contentType = null, int? statusCode = null)
            => Content(content?.ToString(), contentType, statusCode);

        protected ViewResult View(string viewName, object model)
            => Context.RequestServices.GetService<IActionResultProvider<ViewResult>>()
                .GetResult(viewName, model);

        protected ViewResult View(object model)
            => View(null, model);

        protected ViewResult View(string viewName = null)
            => View(viewName, new object());

        protected FileResult File(string path, string contentType, int? statusCode = null)
            => Context.RequestServices.GetService<IActionResultProvider<FileResult>>()
                .GetResult(path, contentType, statusCode);

        protected JSONResult Json(object content, int? statusCode = null)
            => Context.RequestServices.GetService<IActionResultProvider<JSONResult>>()
                .GetResult(content, statusCode);
    }
}