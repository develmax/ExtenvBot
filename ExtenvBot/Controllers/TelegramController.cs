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
                try
                {
                    if (update.Message.Text.Equals("/start", StringComparison.OrdinalIgnoreCase))
                    {
                        _telegramClient.SendMessage(update.Message.Chat.Id, $"Hello! I am TeamCity. Welcome.");
                    }
                    else if (update.Message.Text.StartsWith("/subscribe", StringComparison.OrdinalIgnoreCase))
                    {
                        string envs;

                        if (string.Equals(update.Message.Text.Trim(), "/subscribe", StringComparison.OrdinalIgnoreCase))
                            envs = "all";
                        else
                            envs = update.Message.Text.Substring("/subscribe ".Length).Trim();

                        _storage.Subscribe(update.Message.Chat.Id.ToString(), envs);
                        
                        _telegramClient.SendMessage(update.Message.Chat.Id,
                            $"I subscribe you on envs: \"{envs}\".");
                    }
                    else if (update.Message.Text.StartsWith("/unsubscribe", StringComparison.OrdinalIgnoreCase) ||
                             update.Message.Text.StartsWith("/stop", StringComparison.OrdinalIgnoreCase))
                    {
                        _storage.UnSubscribe(update.Message.Chat.Id.ToString());

                        _telegramClient.SendMessage(update.Message.Chat.Id,
                            $"I unsubscribe you.");

                        if(update.Message.Text.StartsWith("/stop", StringComparison.OrdinalIgnoreCase))
                            _telegramClient.SendMessage(update.Message.Chat.Id,
                                $"Bye. I will see you later.");
                    }
                    else if (update.Message.Text.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
                    {
                        _storage.AdminId = update.Message.Chat.Id;
                        _storage.AdminName = update.Message.Chat.Username;
                        _telegramClient.SendMessage(update.Message.Chat.Id,
                            $"Set admin is " + update.Message.Chat.Username + ".");
                    }
                    else if (_storage.AdminId.HasValue)
                    {
                        _telegramClient.SendMessage(update.Message.Chat.Id,
                            $"Your text translate to TeamCity.");
                        _telegramClient.SendMessage(_storage.AdminId.Value,
                            $"Query text by user " + update.Message.Chat.Username + "(" + (update.Message.Chat.FirstName + " " + update.Message.Chat.LastName).Trim() + ")" + ": " + update.Message.Text);
                    }
                }
                catch (Exception e)
                {
                    _telegramClient.SendMessage(update.Message.Chat.Id,
                        $"Exception: {e.ToString()}");
                    throw;
                }
            }
        }
        
    }
}
