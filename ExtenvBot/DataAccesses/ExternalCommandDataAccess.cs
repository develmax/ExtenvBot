using System;
using System.Linq;
using ExtenvBot.Storages;
using ExtenvBot.Storages.Entities;

namespace ExtenvBot.DataAccesses
{
    public class ExternalCommandDataAccess: IExternalCommandDataAccess
    {
        private IStorage _storage;
        public ExternalCommandDataAccess(IStorage storage)
        {
            _storage = storage;
        }

        public void AddExternalCommand(string id, string chatId, string command, string request)
        {
            var table = _storage.GetTable(ExternalCommandEntity.TableKey);

            _storage.CreateIfNotExists(table);

            var entity = new ExternalCommandEntity(id, DateTime.Now, chatId, command, request);
            _storage.InsertEntity(table, entity);
        }

        public ExternalCommandEntity GetNextExternalCommand()
        {
            var table = _storage.GetTable(ExternalCommandEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return null;

            var entity = _storage.RetrieveEntities<ExternalCommandEntity>(table).Where(i => !i.Processed).OrderBy(i => i.Date).FirstOrDefault();

            return entity;
        }

        public ExternalCommandEntity GetExternalCommand(string id)
        {
            var table = _storage.GetTable(ExternalCommandEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return null;

            var entity = _storage.RetrieveEntity<ExternalCommandEntity>(table, ExternalCommandEntity.Key, id);

            return entity;
        }

        public void SetRequestReceivedExternalCommand(string id)
        {
            var table = _storage.GetTable(ExternalCommandEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return;

            var entity = _storage.RetrieveEntity<ExternalCommandEntity>(table, ExternalCommandEntity.Key, id);

            entity.RequestReceived = true;

            _storage.UpdateEntity(table, entity);
        }

        public void SetResponseExternalCommand(string id, string response)
        {
            var table = _storage.GetTable(ExternalCommandEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return;

            var entity = _storage.RetrieveEntity<ExternalCommandEntity>(table, ExternalCommandEntity.Key, id);

            entity.Response = response;

            _storage.UpdateEntity(table, entity);
        }

        public void ResponseReceivedExternalCommand(string id)
        {
            var table = _storage.GetTable(ExternalCommandEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return;

            var entity = _storage.RetrieveEntity<ExternalCommandEntity>(table, ExternalCommandEntity.Key, id);

            entity.ResponseReceived = true;

            _storage.UpdateEntity(table, entity);
        }

        public void SetProcessedExternalCommand(string id)
        {
            var table = _storage.GetTable(ExternalCommandEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return;

            var entity = _storage.RetrieveEntity<ExternalCommandEntity>(table, ExternalCommandEntity.Key, id);

            entity.Processed = true;

            _storage.UpdateEntity(table, entity);
        }

        public ExternalCommandEntity[] GetExternalCommands()
        {
            var table = _storage.GetTable(ExternalCommandEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return null;

            return _storage.RetrieveEntities<ExternalCommandEntity>(table).ToArray();
        }
    }
}