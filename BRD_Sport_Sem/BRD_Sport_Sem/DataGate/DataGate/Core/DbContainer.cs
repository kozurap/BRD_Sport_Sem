using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataGate.Core
{
    public class DbContainer<T> : IEnumerable<DbObject<T>>
    {
        private DataGateORM _orm;
        private List<DbObject<T>> _dbObjects;
        
        internal DataContext TableContext;

        public int Count => _dbObjects.Count;
        public IEnumerable<T> Values => _dbObjects.Select(o => o.Value);

        public DbContainer(List<DbObject<T>> dbObjects
            , DataContext tableContext, DataGateORM orm)
        {
            _dbObjects = dbObjects;
            TableContext = tableContext;
            _orm = orm;
        }

        public DbObject<T> this[int index]
        {
            get => GetObject(index);
        }

        public DbObject<T> GetObject(int index)
            => _dbObjects[index];

        public void Update()
        {
            var execute = _orm.CurrentTransaction == null;
            if(execute)
                _orm.InitializeTransaction();
            
            foreach (var dbObject in _dbObjects)
                dbObject.Update();

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
            
            foreach (var dbObject in _dbObjects)
                dbObject.Remove();
            
            if (execute)
            {
                _orm.CurrentTransaction.CommandBuilder.ExecuteNonQuery();
                _orm.EndTransaction();
            }
        }

        public IEnumerator<DbObject<T>> GetEnumerator()
        {
            return _dbObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}