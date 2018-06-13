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
    public class TelegramController : Controller
    {
        ITelegramClient _telegramClient;
        private Storage _storage;
        public TelegramController(ITelegramClient telegramClient, Storage storage) {
            _telegramClient = telegramClient;
            _storage = storage;
        }

        // GET api/telegram/update/{token}
        [HttpPost("update/{token}")]
        public void Update([FromRoute]string token, [FromBody]Update update)
        {
            if (token != _telegramClient.GetConfiguration().AuthenticationToken)
                return;

            if (update != null && update.Message != null && !string.IsNullOrEmpty(update.Message.Text))
            {
                if (update.Message.Text.StartsWith("/subsribe ", StringComparison.OrdinalIgnoreCase))
                {
                    var envs = update.Message.Text.Substring("/subsribe ".Length).Trim();

                    _storage.AddSubscription(update.Message.Chat.Id.ToString(), envs);

                    _telegramClient.SendMessage(update.Message.Chat.Id,
                        $"I subscribe you on envs: \"{envs}\"");
                }
                else if (update.Message.Text.StartsWith("/unsubsribe", StringComparison.OrdinalIgnoreCase) ||
                         update.Message.Text.StartsWith("/stop", StringComparison.OrdinalIgnoreCase))
                {
                    _storage.AddSubscription(update.Message.Chat.Id.ToString(), null);

                    _telegramClient.SendMessage(update.Message.Chat.Id,
                        $"I unsubscribe you.");
                }
            }
        }
        
    }
}
