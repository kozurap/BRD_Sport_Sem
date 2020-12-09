﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace ProjectArt.MVCPattern.Services
{
    public class FormValueProviderSource : ValueProviderSource
    {
        public override void Load(HttpContext context)
        {
            var data = new Dictionary<string, StringValues>();
            if(context.Request.HasFormContentType)
                foreach (var pair in context.Request.Form)
                    data.TryAdd(pair.Key, pair.Value);
            Data = data;
        }
    }
}