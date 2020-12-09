using System.Collections.Generic;
using System.Linq;

namespace DataGate.Core
{
    public class DbObject<T>
    {
        internal int DataGateId;
        internal DataContext RowContext;
        private DataGateORM _orm;
        
        public T Value { get; set; }

        public DbObject(T value, DataContext rowContext
            , int dataGateId, DataGateORM orm)
        {
            Value = value;
            RowContext = rowContext;
            DataGateId = dataGateId;
            _orm = orm;
        }

        public void Update()
        {
            var execute = _orm.CurrentTransaction == null;
            if(execute)
                _orm.InitializeTransaction();

            var relationship = DataGateORM.ConnectionContext.Registry.GetRelationship(typeof(T));
            var context = relationship.DbToData(new[] {this});
            var rowContext = context.Single().Value;
            rowContext.Name = RowContext.Name;

            _orm.Executor.CompareAndUpdate(context, RowContext.Parent);
            
            RowContext.Parent.Add(RowContext.Name, rowContext);
            RowContext = rowContext;

            if (execute)
            {
                _orm.CurrentTransaction.CommandBuilder.ExecuteNonQuery();
                _orm.EndTransaction();
            }
        }

        public void Remove()
        {
            var execute = _orm.CurrentTransaction == null;
            if(execute)
                _orm.InitializeTransaction();

            _orm.Remove<T>(DataGateId);

            if (execute)
            {
                _orm.CurrentTransaction.CommandBuilder.ExecuteNonQuery();
                _orm.EndTransaction();
            }
        }
    }
}