using System;
using Microsoft.Azure.CosmosDB.Table;

namespace ExtenvBot.Models
{
    public class SubscriptionEntity : TableEntity
    {
        public const string Key = "Subscription";

        public SubscriptionEntity(string chatId)
        {
            this.PartitionKey = Key;
            this.RowKey = chatId;
        }

        public SubscriptionEntity() { }

        public string ChatId
        {
            get { return this.RowKey; }
        }

        public string Envs { get; set; }
    }
}