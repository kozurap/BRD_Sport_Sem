﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ProjectArt.MVCPattern.Services
{
    public abstract class ValueProviderSource
    {
        private Dictionary<string, StringValues> _data;
        private List<IFormFile> _files;

        public Dictionary<string, StringValues> Data
        {
            get
            {
                if(_data == null)
                    _data = new Dictionary<string, StringValues>();
                return _data;
            }
            set => _data = value;
        }
        
        public List<IFormFile> Files
        {
            get
            {
                if(_files == null)
                    _files = new List<IFormFile>();
                return _files;
            }
            set => _files = value;
        }

        public abstract void Load(HttpContext context);
    }
}