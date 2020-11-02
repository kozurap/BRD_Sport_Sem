﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using ProjectArt.MVCPattern.Services;

namespace ProjectArt.MVCPattern
{
    public class ValueProviderBuilder
    {
        private List<ValueProviderSource> _sources;
        private HttpContext _context;

        public ValueProviderBuilder(HttpContext context)
        {
            _context = context;
            _sources = new List<ValueProviderSource>();
        }

        public void Add<T>() where T : ValueProviderSource
        {
            T source = (T) _context.RequestServices.GetService(typeof(T));
            _sources.Add(source);
        }

        public ValueProvider Build()
        {
            var result = new Dictionary<string, StringValues>();
            foreach (var source in _sources)
            {
                source.Load(_context);
                foreach (var pair in source.Data)
                    result.TryAdd(pair.Key.ToLower(), pair.Value);
            }

            return new ValueProvider(result);
        }
    }
}