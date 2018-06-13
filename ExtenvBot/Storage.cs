using System;
using System.Collections.Generic;
using ExtenvBot.Models;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.Azure.Storage; // Namespace for StorageAccounts
using Microsoft.Azure.CosmosDB.Table; // Namespace for Table storage types

namespace ExtenvBot
{
    public class Storage
    {
        private string _connectionString;

        public Storage(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddSubscription(string chatId, string envs)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Subscriptions");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            // Create a retrieve operation that expects a customer entity.
            TableOperation retrieveOperation =
                TableOperation.Retrieve<SubscriptionEntity>(SubscriptionEntity.Key, chatId.ToString());

            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a CustomerEntity.
            SubscriptionEntity entity = (SubscriptionEntity) retrievedResult.Result;

            if (string.IsNullOrEmpty(envs))
            {
                if (entity != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(entity);

                    // Execute the operation.
                    table.Execute(deleteOperation);
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
                    table.Execute(updateOperation);
                }
                else
                {
                    entity = new SubscriptionEntity(chatId)
                    {
                        Envs = envs
                    };

                    // Create the TableOperation object that inserts the customer entity.
                    TableOperation insertOperation = TableOperation.Insert(entity);

                    // Execute the insert operation.
                    table.Execute(insertOperation);
                }
            }
        }

        public string[] GetSubscription(string env)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("Subscriptions");

            if (!table.Exists()) return null;

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<SubscriptionEntity> query = new TableQuery<SubscriptionEntity>();

            var list = new List<string>();

            // Print the fields for each customer.
            foreach (SubscriptionEntity entity in table.ExecuteQuery(query))
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
    }
}