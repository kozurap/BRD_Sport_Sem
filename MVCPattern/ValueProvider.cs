using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace ProjectArt.MVCPattern.Services
{
    public class ValueProvider : IEnumerable<KeyValuePair<string, StringValues>>
    {
        private readonly Dictionary<string, StringValues> _data;

        public ValueProvider(Dictionary<string, StringValues> data)
        {
            _data = data;
        }

        public string GetValue(string key)
        {
            if (_data.ContainsKey(key))
                return _data[key];
            return null;
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