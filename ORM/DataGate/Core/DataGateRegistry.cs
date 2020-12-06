using System;
using System.Collections.Generic;

namespace DataGate.Core
{
    public class DataGateRegistry
    {
        private TypeRegistry<TableTypeRelationship> _relationshipRegistry;

        public DataGateRegistry()
        {
            _relationshipRegistry = new TypeRegistry<TableTypeRelationship>();
        }

        public void RegisterRelationship(Type type, TableTypeRelationship tableRelationship)
            => _relationshipRegistry.Register(type, tableRelationship);

        public void RegisterRelationship<T>(TableTypeRelationship tableRelationship)
            => _relationshipRegistry.Register<T>(tableRelationship);

        public TableTypeRelationship GetRelationship(Type type)
            => _relationshipRegistry.Get(type);
        
        public TableTypeRelationship GetRelationship<T>()
            =>  _relationshipRegistry.Get<T>();

        public bool Contains(Type type) => _relationshipRegistry.Contains(type);
        public bool Contains<T>() => _relationshipRegistry.Contains<T>();

    }
}