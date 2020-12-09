﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ProjectArt.MVCPattern.Services
{
    public class ValueProvider : IEnumerable<KeyValuePair<string, StringValues>>
    {
        private readonly Dictionary<string, StringValues> _data;
        private readonly ImmutableList<IFormFile> _files;

        public ValueProvider(Dictionary<string, StringValues> data, List<IFormFile> files)
        {
            _data = data;
            _files = files.ToImmutableList();
        }

        public string GetValue(string key)
        {
            if (_data.ContainsKey(key))
                return _data[key];
            return null;
        }

        public ImmutableList<IFormFile> GetFiles()
        {
            return _files;
        }

        public bool Contains(string key) => _data.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}