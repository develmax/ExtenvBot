using ExtenvBot.Models;

namespace ExtenvBot
{
    public interface IStorage
    {
        void UnSubscribe(string chatId);
        long? AdminId { get; set; }
        string AdminName { get; set; }
        void Subscribe(string chatId, string name, string envs);
        string[] GetSubscription(string env);

        string GetEnvs(string chatId);

        ExtenvBot.Models.SubscriptionEntity[] GetSubscriptions();
    }
}