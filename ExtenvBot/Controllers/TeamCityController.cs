using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExtenvBot.DataAccesses;
using ExtenvBot.Storages;
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
        ITelegramBotClient _bot;
        private IDataAccess _dataAccess;
        public TeamCityController(ITelegramBotClient bot, IDataAccess dataAccess) {
            _bot = bot;
            _dataAccess = dataAccess;
        }

        // GET api/telegram/update/{token}
        [HttpPost("send")]
        public void Send([FromBody]ExtenvBot.Models.Message message)
        {
            if (message == null ||
                string.IsNullOrEmpty(message.Env) ||
                string.IsNullOrEmpty(message.Text)) return;

            var subscriptions = _dataAccess.SubscribesDataAccess.GetChatIdsByEnv(message.Env);
            if (subscriptions != null)
            {
                foreach (var subscription in subscriptions)
                {
                    var icon = string.Empty;
                    if(message.Text.Contains("failed", StringComparison.OrdinalIgnoreCase))
                        icon = "\U0000274C" + " ";
                    else if (message.Text.Contains("success", StringComparison.OrdinalIgnoreCase))
                        icon = "\U00002705" + " ";
                    else if (message.Text.Contains("bug", StringComparison.OrdinalIgnoreCase))
                        icon = "\U0001F41B" + " ";
                    else if (message.Text.Contains("create", StringComparison.OrdinalIgnoreCase))
                        icon = "\U0001F4CC" + " ";

                    if (long.TryParse(subscription, out var chatId))
                        _bot.SendTextMessageAsync(chatId, icon + message.Text);
                }
            }
        }
    }
}
