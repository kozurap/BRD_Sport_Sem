using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ProjectArt.MVCPattern.Services
{
    public abstract class ValueProviderSource
    {
        public Dictionary<string, StringValues> _data;

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

        public abstract void Load(HttpContext context);
    }
}