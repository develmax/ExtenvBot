using ExtenvBot.Storages.Entities;

namespace ExtenvBot.DataAccesses
{
    public interface ISubscribesDataAccess
    {
        void UnSubscribe(string chatId);
        void Subscribe(string chatId, string name, string envs);
        string[] GetChatIdsByEnv(string env);
        string GetEnvsByChatId(string chatId);
        SubscribeEntity[] GetSubscribes();
    }
}