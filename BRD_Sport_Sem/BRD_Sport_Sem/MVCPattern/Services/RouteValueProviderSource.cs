﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace ProjectArt.MVCPattern.Services
{
    public class RouteValueProviderSource : ValueProviderSource
    {
        public override void Load(HttpContext context)
        {
            var data = new Dictionary<string, StringValues>();
            foreach (var pair in context.GetRouteData().Values)
                data.TryAdd(pair.Key, (string)pair.Value);
            Data = data;
        }
    }
}