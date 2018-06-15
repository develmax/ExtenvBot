using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace ExtenvBot.Storages.Entities
{
    public class ExternalCommandEntity: TableEntity
    {
        public const string Key = "ExternalCommand";
        public const string TableKey = "ExternalCommands";

        public ExternalCommandEntity(string id, DateTime date, string chatId, string command, string request)
        {
            this.PartitionKey = Key;
            this.RowKey = id;
            this.Date = date;
            this.ChatId = chatId;
            this.Command = command;
            this.Request = request;
        }

        public ExternalCommandEntity() { }

        public string Id => this.RowKey;
        public DateTime Date { get; set; }
        public string ChatId { get; set; }
        public string Command { get; set; }
        public string Request { get; set; }
        public bool RequestReceived { get; set; }
        public string Response { get; set; }
        public bool ResponseReceived { get; set; }
        public bool Processed { get; set; }
    }
}