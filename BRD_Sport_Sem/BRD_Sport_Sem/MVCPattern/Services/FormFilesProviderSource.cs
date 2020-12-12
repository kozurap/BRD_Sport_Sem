﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace ProjectArt.MVCPattern.Services
{
    public class FormFilesProviderSource : ValueProviderSource
    {
        public override void Load(HttpContext context)
        {
            var files = new List<IFormFile>();
            if(context.Request.HasFormContentType)
                foreach (var file in context.Request.Form.Files)
                    files.Add(file);
            Files = files;
        }
    }
}