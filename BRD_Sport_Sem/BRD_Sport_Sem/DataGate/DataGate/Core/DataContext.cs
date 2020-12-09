using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataGate.Core
{
    public enum DataType
    {
        Table,
        Row,
        Field
    }
    
    public class DataContext : IEnumerable<KeyValuePair<object, DataContext>>
    {
        public string Name;
        public DataType Type;
        public DataContext Parent;
        public int Count => _data.Count;
        
        public Dictionary<string, object> Metadata { get; private set; }

        public object Value { get; private set; }

        private Dictionary<object, DataContext> _data;

        public DataContext this[object id]
        {
            get
            {
                if (_data.ContainsKey(id))
                    return _data[id];
                return null;
            }
        }

        private DataContext(string name, DataType type)
        {
            Name = name;
            
            if(string.IsNullOrEmpty(name))
                throw  new ArgumentNullException(nameof(name));
            
            Type = type;

            Metadata = new Dictionary<string, object>();
        }
        
        public DataContext(string name, DataType type, Dictionary<object, DataContext> data) 
            : this(name, type)
        {
            _data = data;
            
            if(data == null)
                _data = new Dictionary<object, DataContext>();

            foreach (var pair in _data)
                pair.Value.Parent = this;

            Value = this;
        }

        public DataContext(string name, DataType type, object value) : this(name, type)
        {
            _data = new Dictionary<object, DataContext>();
            Value = value;
        }

        public IEnumerator<KeyValuePair<object, DataContext>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public void Add(string name, DataContext dataContext)
        {
            dataContext.Parent = this;
            if (_data.ContainsKey(name))
                _data[name] = dataContext;
            else _data.Add(name, dataContext);
        }

        public void Add(DataContext dataContext) => Add(dataContext.Name, dataContext);

        public bool Contains(string name)
        {
            return _data.ContainsKey(name);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}