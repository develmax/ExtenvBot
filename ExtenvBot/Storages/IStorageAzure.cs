using Microsoft.WindowsAzure.Storage.Table;

namespace ExtenvBot.Storages
{
    public interface IStorageAzure: IStorage<CloudTable, TableEntity>
    {

    }
}