using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TelegramBotApi;
using TelegramBotApi.Models;
using TelegramBotApi.Configurations;

namespace ExtenvBot.Controllers
{
    [Route("api/[controller]")]
    public class TeamCityController : Controller
    {
        ITelegramClient _telegramClient;
        private Storage _storage;
        public TeamCityController(ITelegramClient telegramClient, Storage storage) {
            _telegramClient = telegramClient;
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
                    if(long.TryParse(subscription, out var chatId))
                        _telegramClient.SendMessage(chatId, message.Text);
                }
            }
        }
    }
}
