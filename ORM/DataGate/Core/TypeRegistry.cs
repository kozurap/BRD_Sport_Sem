using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DataGate.Core
{
    //TODO: get rid of this class and put it content to Registry class
    internal class TypeRegistry<TValue>
    {
        private ConcurrentDictionary<Type, TValue> _registry;

        public TypeRegistry()
        {
            _registry = new ConcurrentDictionary<Type, TValue>();
        }

        public void Register(Type type, TValue dataBinder)
        {
            if (!_registry.ContainsKey(type))
                _registry.TryAdd(type, dataBinder);
            else _registry[type] = dataBinder;
        }

        public void Register<T>(TValue dataBinder)
            => Register(typeof(T), dataBinder);

        public TValue Get(Type type)
        {
            if(!_registry.ContainsKey(type))
                return default;

            return _registry[type];
        }
        
        public TValue Get<T>()
            => Get(typeof(T));

        public bool Contains(Type type)
            => _registry.ContainsKey(type);

        public bool Contains<T>()
            => _registry.ContainsKey(typeof(T));
    }
}