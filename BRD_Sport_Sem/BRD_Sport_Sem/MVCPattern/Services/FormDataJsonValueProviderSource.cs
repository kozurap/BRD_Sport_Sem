﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProjectArt.MVCPattern.Services
{
    public class FormDataJsonValueProviderSource : ValueProviderSource
    {
        public override void Load(HttpContext context)
        {
            var data = new Dictionary<string, StringValues>();
            if(context.Request.HasFormContentType)
                if (context.Request.Form.ContainsKey("data"))
                {
                    try
                    {
                        dynamic obj = JsonConvert.DeserializeObject(context.Request.Form["data"]);
                        foreach (JProperty property in obj)
                            data.TryAdd(property.Name, property.Value.ToString());
                    }
                    catch
                    {
                        //ignore
                    }
                }

            Data = data;
        }
    }
}