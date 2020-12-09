﻿using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace ProjectArt.MVCPattern.ActionResults
{
    public class ContentResult : IActionResult
    {
        public string Content { get; set; } = null;
        private string _defaultContentType = "text/plain; charset=utf-8";
        public string ContentType { get; set; } = null;
        public int? StatusCode { get; set; } = null;

        public ContentResult(string content = null, string contentType = null, int? statusCode = null)
        {
            Content = content;
            ContentType = contentType;
            StatusCode = statusCode;
        }

        public async Task ExecuteResult(Controller controller)
        {
            var response = controller.Context.Response;
            if (ContentType == null)
                if (!string.IsNullOrEmpty(response.ContentType)) ContentType = response.ContentType;
                else ContentType = _defaultContentType;
            var encoding = MediaType.GetEncoding(ContentType) ?? MediaType.GetEncoding(_defaultContentType);
            response.ContentType = ContentType;
            if(StatusCode != null) response.StatusCode = StatusCode.Value;
            if (Content != null)
            {
                response.ContentLength = encoding.GetByteCount(Content);
                await response.WriteAsync(Content, encoding);
            }

            await response.CompleteAsync();
        }
    }
}