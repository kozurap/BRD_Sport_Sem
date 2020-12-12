﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace ProjectArt.MVCPattern.ActionResults
{
    public class FileResult : IActionResult
    {
        public string Path { get; set; } = null;
        private string _defaultContentType = "application/octet-stream";
        public string ContentType { get; set; } = null;
        public int? StatusCode { get; set; } = null;

        public PhysicalFileProvider FileProvider;

        public FileResult(string path, string contentType, PhysicalFileProvider fileProvider, int? statusCode = null)
        {
            Path = path;
            
            if(string.IsNullOrEmpty(path)) 
                throw new ArgumentNullException(nameof(path));
            
            ContentType = contentType;

            StatusCode = statusCode;
            FileProvider = fileProvider;
        }

        public async Task ExecuteResult(Controller controller)
        {
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            var response = controller.Context.Response;
            if (string.IsNullOrWhiteSpace(ContentType))
                if (!string.IsNullOrEmpty(response.ContentType)) ContentType = response.ContentType;
                else if (contentTypeProvider.TryGetContentType(Path, out var resolvedContentType))
                    ContentType = resolvedContentType;
                else ContentType = _defaultContentType;
            response.ContentType = ContentType;
            if (StatusCode != null)  response.StatusCode = StatusCode.Value;
            
            using (var fileStream = FileProvider.GetFileInfo(Path).CreateReadStream())
            {
                response.ContentLength = fileStream.Length;
                await fileStream.CopyToAsync(response.Body);
            }

            await response.CompleteAsync();
        }
    }
}