using Microsoft.WindowsAzure.Storage.Table;

namespace ExtenvBot.Storages.Entities
{
    public class SettingEntity : TableEntity
    {
        public const string Key = "Setting";
        public const string TableKey = "Settings";

        public SettingEntity(string name, string value)
        {
            this.PartitionKey = Key;
            this.RowKey = name;
            this.Value = value;
        }

        public SettingEntity() { }

        public string Name
        {
            get { return this.RowKey; }
        }

        public string Value { get; set; }
    }
}