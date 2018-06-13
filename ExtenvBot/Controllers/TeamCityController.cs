using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot;
using TelegramBotApi;
using TelegramBotApi.Models;
using TelegramBotApi.Configurations;

namespace ExtenvBot.Controllers
{
    [Route("api/[controller]")]
    public class TeamCityController : Controller
    {
        //ITelegramClient _telegramClient;
        ITelegramBotClient _bot;
        private IStorage _storage;
        public TeamCityController(ITelegramBotClient bot /*ITelegramClient telegramClient*/, IStorage storage) {
            //_telegramClient = telegramClient;
            _bot = bot;
            _storage = storage;
        }

        // GET api/telegram/update/{token}
        [HttpPost("send")]
        public void Send([FromBody]ExtenvBot.Models.Message message)
        {
            if (message == null ||
                string.IsNullOrEmpty(message.Env) ||
                string.IsNullOrEmpty(message.Text)) return;

            var subscriptions = _storage.GetSubscription(message.Env);
            if (subscriptions != null)
            {
                foreach (var subscription in subscriptions)
                {
                    var icon = string.Empty;
                    if(message.Text.Contains("failed", StringComparison.OrdinalIgnoreCase))
                        icon = "\U0000274C" + " ";
                    else if (message.Text.Contains("success", StringComparison.OrdinalIgnoreCase))
                        icon = "\U00002705" + " ";

                    if (long.TryParse(subscription, out var chatId))
                        //_telegramClient.SendMessage(chatId, message.Text);
                        _bot.SendTextMessageAsync(chatId, icon + message.Text);
                }
            }
        }
    }
}
