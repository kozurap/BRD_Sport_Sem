﻿using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectArt.MVCPattern
{
    public interface IControllerActivator
    {
        public object Activate(HttpContext context, Route endRoute, ObjectFactory factory, ModelBindingState modelState);
    }
}