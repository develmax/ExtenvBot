using System;
using Microsoft.WindowsAzure.Storage.Table;

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

        public string Name { get; set; }
        public string Envs { get; set; }
    }
}