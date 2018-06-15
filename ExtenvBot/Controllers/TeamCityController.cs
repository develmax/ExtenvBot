using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExtenvBot.DataAccesses;
using ExtenvBot.Models;
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

        public TeamCityController(ITelegramBotClient bot, IDataAccess dataAccess)
        {
            _bot = bot;
            _dataAccess = dataAccess;
        }

        // GET api/telegram/update/{token}
        [HttpPost("send")]
        public void Send([FromBody] ExtenvBot.Models.Message message)
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
                    if (message.Text.Contains("failed", StringComparison.OrdinalIgnoreCase))
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

        [HttpGet("getexternalcommand")]
        public IActionResult GetExternalCommand()
        {
            var command = _dataAccess.ExternalCommandDataAccess.GetNextExternalCommand();
            if (command != null)
            {
                return new ContentResult()
                {
                    Content = $"{{ \"id\":\"{command.Id}\", \"command\":\"{command.Command}\", \"request\":{ (string.IsNullOrEmpty(command.Request) ? "\"\"" : command.Request) } }}",
                    ContentType = "application/json",
                    StatusCode = 200
                };
                //return Json(new ExternalCommand(){ Id = command.Id, Command = command.Command, Request = command.Request } );

            }

            return Ok();
        }

        [HttpPost("setexternalcommand")]
        public IActionResult SetExternalCommand([FromBody] ExtenvBot.Models.ExternalCommandResult result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Id))
            {
                var command = _dataAccess.ExternalCommandDataAccess.GetExternalCommand(result.Id);
                if (command != null)
                {
                    _dataAccess.ExternalCommandDataAccess.SetResponseExternalCommand(result.Id, result.Response);
                    
                    _bot.SendTextMessageAsync(command.ChatId, "Your request id = '"+ result.Id + "' processed. [Show]("+
                                                              "http://extenvbot.azurewebsites.net/api/telegram/externalcommand/"+ result.Id
                                                              + ").",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

                    _dataAccess.ExternalCommandDataAccess.SetProcessedExternalCommand(result.Id);
                }
            }

            return Ok();
        }
    }
}
