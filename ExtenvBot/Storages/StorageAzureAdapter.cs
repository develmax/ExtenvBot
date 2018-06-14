using System.Collections.Generic;
using System.Reflection;
using Microsoft.WindowsAzure.Storage.Table;

namespace ExtenvBot.Storages
{
    public class StorageAzureAdapter: IStorage
    {
        private IStorageAzure _storageAzure;

        public StorageAzureAdapter(IStorageAzure storageAzure)
        {
            _storageAzure = storageAzure;
        }

        public object GetTable(string name)
        {
            return _storageAzure.GetTable(name);
        }

        public bool IsExistsTable(object table)
        {
            return _storageAzure.IsExistsTable((CloudTable)table);
        }

        public void CreateIfNotExists(object table)
        {
            _storageAzure.CreateIfNotExists((CloudTable)table);
        }

        public T RetrieveEntity<T>(object table, string partitionKey, string rowkey)
        {
            var method = typeof(IStorageAzure).GetMethod("RetrieveEntity");
            var generic = method.MakeGenericMethod(typeof(T));
            var entity = generic.Invoke(_storageAzure, new object[]{ (CloudTable)table, partitionKey, rowkey });

            return entity is T ? (T)entity : default(T);
        }

        public void DeleteEntity<T>(object table, T entity)
        {
            var method = typeof(IStorageAzure).GetMethod("DeleteEntity");
            var generic = method.MakeGenericMethod(typeof(T));
            generic.Invoke(_storageAzure, new object[] { (CloudTable)table, entity });
        }

        public void UpdateEntity<T>(object table, T entity)
        {
            var method = typeof(IStorageAzure).GetMethod("UpdateEntity");
            var generic = method.MakeGenericMethod(typeof(T));
            generic.Invoke(_storageAzure, new object[] { (CloudTable)table, entity });
        }

        public void InsertEntity<T>(object table, T entity)
        {
            var method = typeof(IStorageAzure).GetMethod("InsertEntity");
            var generic = method.MakeGenericMethod(typeof(T));
            generic.Invoke(_storageAzure, new object[] { (CloudTable)table, entity });
        }

        public IEnumerable<T> RetrieveEntities<T>(object table)
        {
            var method = typeof(IStorageAzure).GetMethod("RetrieveEntities");
            var generic = method.MakeGenericMethod(typeof(T));

            var entities = generic.Invoke(_storageAzure, new object[] { (CloudTable)table });

            return entities is IEnumerable<T> ? (IEnumerable<T>)entities : default(IEnumerable<T>);
        }
    }
}