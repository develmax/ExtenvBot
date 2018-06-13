using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExtenvBot.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

// Namespace for Table storage types

namespace ExtenvBot
{
    public class StorageAzure: IStorage
    {
        private string _connectionString;

        public StorageAzure(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void UnSubscribe(string chatId)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Subscriptions");

            Task<bool> existsAsyncResult = null;
            Task.Run(() =>
            {
                existsAsyncResult = table.ExistsAsync();
            }).Wait();

            if (!existsAsyncResult.Result) return;

            // Create the table if it doesn't exist.
            //table.CreateIfNotExistsAsync();
            //table.CreateIfNotExists();

            // Create a retrieve operation that expects a customer entity.
            TableOperation retrieveOperation =
                TableOperation.Retrieve<SubscriptionEntity>(SubscriptionEntity.Key, chatId.ToString());

            // Execute the operation.
            TableResult retrievedResult = null;
            Task<TableResult> taskRetrievedResult = null;
            // Execute the operation.
            Task.Run(() =>
            {
                taskRetrievedResult = table.ExecuteAsync(retrieveOperation);
            }).Wait();

            retrievedResult = taskRetrievedResult.Result;

            // Assign the result to a CustomerEntity.
            SubscriptionEntity entity = (SubscriptionEntity)retrievedResult.Result;

            if (entity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(entity);

                // Execute the operation.
                Task.Run(() => table.ExecuteAsync(deleteOperation)).Wait();
            }
        }

        public long? AdminId { get; set; }
        public string AdminName { get; set; }

        public string GetEnvs(string chatId)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Subscriptions");

            Task<bool> existsAsyncResult = null;
            Task.Run(() =>
            {
                existsAsyncResult = table.ExistsAsync();
            }).Wait();

            if (!existsAsyncResult.Result) return null;

            // Create a retrieve operation that expects a customer entity.
            TableOperation retrieveOperation =
                TableOperation.Retrieve<SubscriptionEntity>(SubscriptionEntity.Key, chatId.ToString());

            TableResult retrievedResult = null;
            Task<TableResult> taskRetrievedResult = null;
            // Execute the operation.
            Task.Run(() =>
            {
                taskRetrievedResult = table.ExecuteAsync(retrieveOperation);
            }).Wait();

            retrievedResult = taskRetrievedResult.Result;

            // Assign the result to a CustomerEntity.
            SubscriptionEntity entity = (SubscriptionEntity)retrievedResult.Result;

            return entity?.Envs;
        }

        public void Subscribe(string chatId, string name, string envs)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
            CloudTableClient tableClient = new CloudTableClient(storageAccount.TableStorageUri.PrimaryUri, storageAccount.Credentials);

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Subscriptions");

            // Create the table if it doesn't exist.
            Task.Run(() => table.CreateIfNotExistsAsync()).Wait();

            // Create a retrieve operation that expects a customer entity.
            TableOperation retrieveOperation =
                TableOperation.Retrieve<SubscriptionEntity>(SubscriptionEntity.Key, chatId.ToString());

            TableResult retrievedResult = null;
            Task<TableResult> taskRetrievedResult = null;
            // Execute the operation.
            Task.Run(() =>
            {
                taskRetrievedResult = table.ExecuteAsync(retrieveOperation);
            }).Wait();

            retrievedResult = taskRetrievedResult.Result;

            // Assign the result to a CustomerEntity.
            SubscriptionEntity entity = (SubscriptionEntity) retrievedResult.Result;

            if (string.IsNullOrEmpty(envs))
            {
                if (entity != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(entity);

                    // Execute the operation.
                    Task.Run(() => table.ExecuteAsync(deleteOperation)).Wait();
                }
            }
            else
            {
                if (entity != null)
                {
                    entity.Envs = envs;

                    // Create the Replace TableOperation.
                    TableOperation updateOperation = TableOperation.Replace(entity);

                    // Execute the operation.
                    Task.Run(() => table.ExecuteAsync(updateOperation)).Wait();
                }
                else
                {
                    entity = new SubscriptionEntity(chatId)
                    {
                        Name = name,
                        Envs = envs
                    };

                    // Create the TableOperation object that inserts the customer entity.
                    TableOperation insertOperation = TableOperation.Insert(entity);

                    // Execute the insert operation.
                    Task.Run(() => table.ExecuteAsync(insertOperation)).Wait();
                }
            }
        }

        public string[] GetSubscription(string env)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Subscriptions");

            Task<bool> existsAsyncResult = null;
            Task.Run(() =>
            {
                existsAsyncResult = table.ExistsAsync();
            }).Wait();
               
            if (!existsAsyncResult.Result) return null;

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<SubscriptionEntity> query = new TableQuery<SubscriptionEntity>();

            var list = new List<string>();

            Task<TableQuerySegment<SubscriptionEntity>> taskQueryResult = null;

            Task.Run(() =>
            {
                taskQueryResult = table.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());
            }).Wait();
            

            // Print the fields for each customer.
            foreach (SubscriptionEntity entity in taskQueryResult.Result)
            {
                if (!string.IsNullOrEmpty(entity.Envs))
                {
                    var i = entity.Envs.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in i)
                    {
                        if (string.Equals(s, "all", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s, env, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!list.Contains(entity.ChatId))
                                list.Add(entity.ChatId);
                            break;
                        }
                    }
                }
            }

            return list.Count > 0 ? list.ToArray() : null;
        }

        public ExtenvBot.Models.SubscriptionEntity[] GetSubscriptions()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Subscriptions");

            Task<bool> existsAsyncResult = null;
            Task.Run(() =>
            {
                existsAsyncResult = table.ExistsAsync();
            }).Wait();

            if (!existsAsyncResult.Result) return null;

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<SubscriptionEntity> query = new TableQuery<SubscriptionEntity>();

            var list = new List<string>();

            Task<TableQuerySegment<SubscriptionEntity>> taskQueryResult = null;

            Task.Run(() =>
            {
                taskQueryResult = table.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());
            }).Wait();

            return taskQueryResult.Result.ToArray();
        }
    }
}