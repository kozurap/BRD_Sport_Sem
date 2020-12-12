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
    public class StreamResult : IActionResult
    {
        public string Path { get; set; } = null;
        private string _defaultContentType = "application/octet-stream";
        public string ContentType { get; set; } = null;
        public int? StatusCode { get; set; } = null;

        public Stream Stream;

        public StreamResult(Stream stream, string contentType, int? statusCode = null)
        {
            ContentType = contentType;

            StatusCode = statusCode;
            Stream = stream;
        }

        public async Task ExecuteResult(Controller controller)
        {
            var response = controller.Context.Response;


            if (string.IsNullOrWhiteSpace(ContentType))
                ContentType = _defaultContentType;
            response.ContentType = ContentType;
            if (StatusCode != null)  response.StatusCode = StatusCode.Value;
            response.ContentLength = Stream.Length;
            await Stream.CopyToAsync(response.Body);
            Stream.Close();
            
            await response.CompleteAsync();
        }
    }
}