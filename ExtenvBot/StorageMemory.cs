using System;
using System.Collections.Generic;
using System.Linq;
using ExtenvBot.Models;

namespace ExtenvBot
{
    public class StorageMemory : IStorage
    {
        private List<SubscriptionEntity> _list = new List<SubscriptionEntity>();

        public void UnSubscribe(string chatId)
        {
            var entity = _list.FirstOrDefault(i => string.Equals(i.ChatId, chatId, StringComparison.OrdinalIgnoreCase));

            if (entity != null)
            {
                _list.Remove(entity);
            }
        }

        public long? AdminId { get; set; }
        public string AdminName { get; set; }

        public void Subscribe(string chatId, string name, string envs)
        {
            var entity = _list.FirstOrDefault(i => string.Equals(i.ChatId, chatId, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(envs))
            {
                if (entity != null)
                {
                    _list.Remove(entity);
                }
            }
            else
            {
                if (entity != null)
                {
                    entity.Envs = envs;
                }
                else
                {
                    _list.Add(new SubscriptionEntity(chatId) {Name = name, Envs = envs});
                }
            }
        }

        public string[] GetSubscription(string env)
        {
            if (_list.Count == 0) return null;

            var list = new List<string>();

            foreach (SubscriptionEntity entity in _list)
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

        public string GetEnvs(string chatId)
        {
            if (_list.Count == 0) return null;

            var envs = _list.FirstOrDefault(i => i.ChatId == chatId);

            return envs?.Envs;
        }

        public ExtenvBot.Models.SubscriptionEntity[] GetSubscriptions()
        {
            if (_list.Count == 0) return null;

            var envs = _list.ToArray();

            return envs;
        }
    }
}