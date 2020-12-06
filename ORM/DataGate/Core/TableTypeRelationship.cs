using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataGate.Core.Attributes;
using DataGate.Utils;

namespace DataGate.Core
{
    public class TableTypeRelationship
    {
        public string TableName;
        public Type EntityType;
        
        public string Id { get; private set; }

        private List<VariableInfo> _variables;
        private VariableInfo _id;

        public static string GetDbIdentifier(MemberInfo memberInfo)
        {
            string dbIdentifier = memberInfo.Name.ToLower();

            dbIdentifier = memberInfo.GetCustomAttribute<DataRelationAttribute>()?.DbIdentifier ??
                           dbIdentifier;
            
            return dbIdentifier;
        }
        
        public TableTypeRelationship(string tableName, Type entityType)
        {
            TableName = tableName;
            EntityType = entityType;
            
            _variables = new List<VariableInfo>();
            foreach (var variable in entityType.GetVariables())
            {
                _variables.Add(variable);
                
                if(variable.MemberInfo.GetCustomAttribute<IdAttribute>() != null)
                    if (_id != null)
                        throw new Exception("Two field or properties with Id Attribute");
                    else _id = variable;
            }

            if (_id == null)
                Id = "datagate_id";
            else
                Id = GetDbIdentifier(_id.MemberInfo);

            if(entityType.GetConstructor(Type.EmptyTypes) == null)
                throw new Exception("Type must have a parameterless constructor");
        }

        //TODO: Entity caching
        public IEnumerable<DbObject<T>> ToDbObjects<T>(DataContext dataContext, DataGateORM orm)
        {
            foreach (var row in dataContext)
            {
                var obj = Activator.CreateInstance(EntityType);
                foreach (var variable in _variables)
                {
                    var memberInfo = variable.MemberInfo;
                    var dbIdentifier = GetDbIdentifier(memberInfo); 

                    var value = row.Value[dbIdentifier].Value;
                    //TODO: resolve Nullable type
                    try
                    {
                        object parsedValue = Convert.ChangeType(value, variable.VariableType);
                        variable[obj] = parsedValue;
                    }
                    catch
                    {
                        //ignore
                    }
                }

                var dbObj = new DbObject<T>((T) obj, row.Value, (int) (row.Value["datagate_id"]?.Value ?? -1), orm);
                yield return dbObj;
            }
        }

        public DataContext DbToData<T>(IEnumerable<DbObject<T>> entities)
        {
            if(typeof(T) != EntityType)
                throw new ArgumentException($"Entities should be of type '{EntityType}");
            
            var tableContext = new DataContext(TableName, DataType.Table, null);
            int index = 0;
            foreach (var entity in entities)
            {
                var rowContext = new DataContext(index.ToString(), DataType.Row, null);
                rowContext.Add(new DataContext("datagate_id", DataType.Field, entity.DataGateId));
                var obj = entity.Value;
                foreach (var variable in _variables)
                {
                    var memberInfo = variable.MemberInfo;
                    string dbIdentifier = memberInfo.Name.ToLower();

                    dbIdentifier = memberInfo.GetCustomAttribute<DataRelationAttribute>()?.DbIdentifier ??
                                   dbIdentifier;

                    rowContext.Add(new DataContext(dbIdentifier, DataType.Field, variable.Get(obj)));
                }
                
                tableContext.Add(rowContext);
            }

            return tableContext;
        }
        
        public DataContext ToData<T>(IEnumerable<T> entities)
        {
            if(typeof(T) != EntityType)
                throw new ArgumentException($"Entities should be of type '{EntityType}");
            
            var tableContext = new DataContext(TableName, DataType.Table, null);
            int index = 0;
            foreach (var entity in entities)
            {
                var rowContext = new DataContext(index.ToString(), DataType.Row, null);
                var obj = entity;
                foreach (var variable in _variables)
                {
                    var memberInfo = variable.MemberInfo;
                    string dbIdentifier = memberInfo.Name.ToLower();

                    dbIdentifier = memberInfo.GetCustomAttribute<DataRelationAttribute>()?.DbIdentifier ??
                                   dbIdentifier;

                    if(memberInfo.GetCustomAttribute<IdAttribute>() == null &&
                       memberInfo.GetCustomAttribute<DefaultOnInsertAttribute>() == null)
                        rowContext.Add(new DataContext(dbIdentifier, DataType.Field, variable.Get(obj)));
                }
                
                tableContext.Add(rowContext);
            }

            return tableContext;
        }
    }
}