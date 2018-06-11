using TelegramBotApi.Configurations;
using TelegramBotApi.Enums;
using TelegramBotApi.Models;

namespace TelegramBotApi
{
    public interface ITelegramClient
    {
        TelegramClientConfiguration GetConfiguration();
        Message SendMessage(long chat_id, string text);
        Update[] GetUpdates(int? offset, int? limit, int? timeout, UpdateType[] allowedUpdates = null);
        bool SetWebhook(string url, int maxConnections, UpdateType[] allowedUpdates = null);
    }
}
