﻿using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectArt.MVCPattern
{
    public class DefaultControllerActivator : IControllerActivator
    {
        public object Activate(HttpContext context, Route endRoute, ObjectFactory factory, ModelBindingState modelState)
        {
            var routeData = context.GetRouteData();
            var controllerObject = factory(context.RequestServices, new object[0]);
            var controller = (Controller) controllerObject;
            controller.Context = context;
            controller.RouteInfo = endRoute;
            controller.RouteData = routeData;
            controller.RouteValues = routeData.Values;
            controller.Request = context.Request;
            controller.Response = context.Response;
            controller.ModelBindingState = modelState;
            controller.Initialize();

            return controller;
        }
    }
}