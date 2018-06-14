using System;
using System.Collections.Generic;
using System.Linq;
using ExtenvBot.Storages;
using ExtenvBot.Storages.Entities;

namespace ExtenvBot.DataAccesses
{
    public class SubscribesDataAccess: ISubscribesDataAccess
    {
        private IStorage _storage;
        public SubscribesDataAccess(IStorage storage)
        {
            _storage = storage;
        }

        public void UnSubscribe(string chatId)
        {
            var table = _storage.GetTable(SubscribeEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return;

            var entity = _storage.RetrieveEntity<SubscribeEntity>(table, SubscribeEntity.Key, chatId);
            if (entity != null)
            {
                _storage.DeleteEntity(table, entity);
            }
        }

        public string GetEnvsByChatId(string chatId)
        {
            var table = _storage.GetTable(SubscribeEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return null;

            var entity = _storage.RetrieveEntity<SubscribeEntity>(table, SubscribeEntity.Key, chatId);

            return entity?.Envs;
        }

        public void Subscribe(string chatId, string name, string envs)
        {
            var table = _storage.GetTable(SubscribeEntity.TableKey);

            _storage.CreateIfNotExists(table);

            var entity = _storage.RetrieveEntity<SubscribeEntity>(table, SubscribeEntity.Key, chatId);

            if (string.IsNullOrEmpty(envs))
            {
                if (entity != null) _storage.DeleteEntity(table, entity);
            }
            else
            {
                if (entity != null)
                {
                    entity.Envs = envs;

                    _storage.UpdateEntity(table, entity);
                }
                else
                {
                    entity = new SubscribeEntity(chatId)
                    {
                        Name = name,
                        Envs = envs
                    };

                    _storage.InsertEntity(table, entity);
                }
            }
        }

        public string[] GetChatIdsByEnv(string env)
        {
            var table = _storage.GetTable(SubscribeEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return null;

            var list = new List<string>();

            var entities = _storage.RetrieveEntities<SubscribeEntity>(table);

            // Print the fields for each customer.
            foreach (var entity in entities)
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

        public SubscribeEntity[] GetSubscribes()
        {
            var table = _storage.GetTable(SubscribeEntity.TableKey);

            if (!_storage.IsExistsTable(table)) return null;

            return _storage.RetrieveEntities<SubscribeEntity>(table).ToArray();
        }
    }
}