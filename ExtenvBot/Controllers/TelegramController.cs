using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ExtenvBot.DataAccesses;
using ExtenvBot.Models;
using ExtenvBot.Storages;
using ExtenvBot.Storages.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ExtenvBot.Controllers
{
    [Route("api/[controller]")]
    public class TelegramController : Controller
    {
        ITelegramBotClient _bot;
        private IDataAccess _dataAccess;
        private ISettings _settings;
        private IConfiguration _configuration;
        public TelegramController(ITelegramBotClient bot, IDataAccess dataAccess,
            ISettings settings, IConfiguration configuration) {
            _bot = bot;
            _dataAccess = dataAccess;
            _settings = settings;
            _configuration = configuration;
        }

        [HttpGet("subscribes")]
        public IActionResult Subscribes()
        {
            var subscriptions = _dataAccess.SubscribesDataAccess.GetSubscribes();

            return View(new SubscriptionsViewModel { EntityList = subscriptions != null ? subscriptions.ToList() : new List<SubscribeEntity>() });
        }

        [HttpGet("settings")]
        public IActionResult Settings()
        {
            var settings = _dataAccess.SettingsDataAccess.GetSettings();

            return View(new SettingsViewModel { EntityList = settings != null ? settings.ToList() : new List<SettingEntity>() });
        }

        [HttpGet("externalcommands")]
        public IActionResult ExternalCommands()
        {
            var commands = _dataAccess.ExternalCommandDataAccess.GetExternalCommands();

            return View(new ExternalCommandsViewModel { EntityList = commands != null ? commands.ToList() : new List<ExternalCommandEntity>() });
        }

        [HttpGet("externalcommand/{id}")]
        public IActionResult ExternalCommand(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var command = _dataAccess.ExternalCommandDataAccess.GetExternalCommand(id);
                if (command != null)
                {
                    return View(new ExternalCommandViewModel() { Entity = command });
                }
            }

            return Ok();
        }

        // GET api/telegram/update/{token}
        [HttpPost("update/{token}")]
        public void Update([FromRoute] string token, [FromBody] Telegram.Bot.Types.Update update)
        {
            try
            {
                var accessToken = _configuration["Settings:accessToken"];
                if (token != accessToken) return;

                if (update == null) return;

                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    if (update.CallbackQuery != null && !string.IsNullOrEmpty(update.CallbackQuery.Data) &&
                        update.CallbackQuery.Message != null && !string.IsNullOrEmpty(update.CallbackQuery.Message.Text))
                    {
                        ProcessCallbackQuery(update);
                    }
                }
                else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    if (update.Message != null && !string.IsNullOrEmpty(update.Message.Text))
                    {
                        ProcessMessage(update);
                    }
                }

            }
            catch (Exception e)
            {
                _bot.SendTextMessageAsync(update.Message.Chat.Id,
                    $"Exception: {e.ToString()}");
                //throw;
            }
        }

        private void ProcessMessage(Update update)
        {
            if (update.Message.Text.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                StartCommand(update);
            }
            else if (update.Message.Text.Equals("/subscribe", StringComparison.OrdinalIgnoreCase))
            {
                SubscribeCommand(update);
            }
            else if (update.Message.Text.Equals("/unsubscribe", StringComparison.OrdinalIgnoreCase))
            {
                UnSubscribeCommand(update);
            }
            else if (update.Message.Text.Equals("/stop", StringComparison.OrdinalIgnoreCase))
            {
                StopCommand(update);
            }
            else if (update.Message.Text.Equals("/subscribes", StringComparison.OrdinalIgnoreCase))
            {
                SubscribesCommand(update);
            }
            else if (update.Message.Text.Equals("/tasks", StringComparison.OrdinalIgnoreCase))
            {
                TasksCommand(update);
            }
            else if (update.Message.Text.Equals("/tasklist", StringComparison.OrdinalIgnoreCase))
            {
                TaskListCommand(update);
            }
            else if (update.Message.Text.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
            {
                AdminCommand(update);
            }
            else
            {
                ProcessPlanText(update);
            }
        }

        private void ProcessPlanText(Update update)
        {
            if (_settings.AdminId.HasValue)
            {
                _bot.SendTextMessageAsync(update.Message.Chat.Id, $"Your text translate to TeamCity.");

                _bot.SendTextMessageAsync(_settings.AdminId.Value,
                    $"Query text by user " + update.Message.Chat.Username + " (" +
                    (update.Message.Chat.FirstName + " " + update.Message.Chat.LastName).Trim() + ")" +
                    ": " + update.Message.Text);
            }
        }

        private void AdminCommand(Update update)
        {
            _settings.AdminId = update.Message.Chat.Id;
            _settings.AdminName = update.Message.Chat.Username;

            _bot.SendTextMessageAsync(update.Message.Chat.Id,
                $"Set admin is " + update.Message.Chat.Username + ".");
        }

        private void SubscribesCommand(Update update)
        {
            var envs = _dataAccess.SubscribesDataAccess.GetEnvsByChatId(update.Message.Chat.Id.ToString());

            var text = string.IsNullOrEmpty(envs)
                ? "Subscribe envs is not found."
                : "Subscribe envs: " + envs.Replace(",", ", ") + ".";
            _bot.SendTextMessageAsync(update.Message.Chat.Id, text);
        }

        private void TasksCommand(Update update)
        {
            var id = Guid.NewGuid().ToString().Substring(0, 8);
            _dataAccess.ExternalCommandDataAccess.AddExternalCommand(id, update.Message.Chat.Id.ToString(), "tasks", null);

            //_bot.SendTextMessageAsync(update.Message.Chat.Id, $"Your request with id = '{id}' add in queue.");
            _bot.SendTextMessageAsync(update.Message.Chat.Id, $"Request tasks...");
        }

        private void TaskListCommand(Update update)
        {
            var id = Guid.NewGuid().ToString().Substring(0, 8);
            _dataAccess.ExternalCommandDataAccess.AddExternalCommand(id, update.Message.Chat.Id.ToString(), "tasklist", null);

            //_bot.SendTextMessageAsync(update.Message.Chat.Id, $"Your request with id = '{id}' add in queue.");
            _bot.SendTextMessageAsync(update.Message.Chat.Id, $"Prepare task list...");
        }

        private void StopCommand(Update update)
        {
            _dataAccess.SubscribesDataAccess.UnSubscribe(update.Message.Chat.Id.ToString());

            _bot.SendTextMessageAsync(update.Message.Chat.Id, $"I unsubscribe you.");
            _bot.SendTextMessageAsync(update.Message.Chat.Id, $"Bye. I will see you later.");
        }

        private void UnSubscribeCommand(Update update)
        {
            var envs = _dataAccess.SubscribesDataAccess.GetEnvsByChatId(update.Message.Chat.Id.ToString());

            var envsList = !string.IsNullOrEmpty(envs)
                ? envs.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();

            var lines = new List<List<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton>>();

            var max = 23;
            int j = 1;
            for (int i = 10; i <= max + 2; i++)
            {
                if (i == 14 || i == 15) continue;

                var name = i <= max ? "crm" + i.ToString() : (i == max + 1 ? "preprod" : "all");

                if (envsList.Contains(name) || (name == "all"))
                {
                    if (lines.Count < j)
                        lines.Add(new List<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton>());

                    lines[j - 1].Add(Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton
                        .WithCallbackData(name));

                    if (lines[j - 1].Count == 4) j++;
                }
            }

            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(lines);

            _bot.SendTextMessageAsync(update.Message.Chat.Id, "Unsubscribe envs:",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Default,
                replyMarkup: keyboard);
        }

        private void SubscribeCommand(Update update)
        {
            var envs = _dataAccess.SubscribesDataAccess.GetEnvsByChatId(update.Message.Chat.Id.ToString());

            var envsList = !string.IsNullOrEmpty(envs)
                ? envs.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();

            var lines = new List<List<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton>>();

            var max = 23;
            int j = 1;
            for (int i = 10; i <= max + 2; i++)
            {
                if (i == 14 || i == 15) continue;

                var name = i <= max ? "crm" + i.ToString() : (i == max + 1 ? "preprod" : "all");

                if (!envsList.Contains(name))
                {
                    if (lines.Count < j)
                        lines.Add(new List<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton>());

                    var text = name; // == "preprod" ? "\U0001F699" + name : "\U0001F697" + name;

                    lines[j - 1].Add(Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton
                        .WithCallbackData(text, name));

                    if (lines[j - 1].Count == 4) j++;
                }
            }

            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(lines);

            _bot.SendTextMessageAsync(update.Message.Chat.Id, "Subscribe envs:",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Default,
                replyMarkup: keyboard);
        }

        private void StartCommand(Update update)
        {
            _bot.SendTextMessageAsync(update.Message.Chat.Id,
                $"Hello! I am TeamCity. Welcome!" + Environment.NewLine + Environment.NewLine +
                "Commands:" + Environment.NewLine +
                "/subscribe - Subscribe on all env updates." + Environment.NewLine +
                "/unsubscribe - Unsubscribe on all env updates." + Environment.NewLine +
                "/list - List subscribe envs." + Environment.NewLine + Environment.NewLine +
                "\U0001F609" + " Write your features in this chat.");
        }

        private void ProcessCallbackQuery(Update update)
        {
            if (string.Equals(update.CallbackQuery.Message.Text, "subscribe envs:", StringComparison.OrdinalIgnoreCase))
            {
                SubscribeEnvsCallback(update);
            }
            else if (string.Equals(update.CallbackQuery.Message.Text, "unsubscribe envs:", StringComparison.OrdinalIgnoreCase))
            {
                UnSubscribeEnvsCallback(update);
            }
            else if (string.Equals(update.CallbackQuery.Message.Text, "task list:", StringComparison.OrdinalIgnoreCase))
            {
                TaskListCallback(update);
            }
            else if (update.CallbackQuery.Message.Text.StartsWith("extenv", StringComparison.OrdinalIgnoreCase))
            {
                ExtenvCallback(update);
            }
        }

        private void UnSubscribeEnvsCallback(Update update)
        {
            string envs = null;

            if (update.CallbackQuery.Data == "all")
            {
                _dataAccess.SubscribesDataAccess.UnSubscribe(update.CallbackQuery.Message.Chat.Id.ToString());
                _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                    $"Select " + update.CallbackQuery.Data + ". " + $"I unsubscribe you.");
            }
            else
            {
                envs = _dataAccess.SubscribesDataAccess.GetEnvsByChatId(update.CallbackQuery.Message.Chat.Id.ToString());
                if (!string.IsNullOrEmpty(envs))
                {
                    var envsList = envs.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (envsList.Contains(update.CallbackQuery.Data))
                        envsList.Remove(update.CallbackQuery.Data);

                    if (envsList.Count > 0)
                        envs = string.Join(',', envsList.ToArray());
                }

                if (string.IsNullOrEmpty(envs))
                {
                    _dataAccess.SubscribesDataAccess.UnSubscribe(update.CallbackQuery.Message.Chat.Id.ToString());
                    _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                        $"Select " + update.CallbackQuery.Data + ". " + $"I unsubscribe you.");
                }
                else
                {
                    _dataAccess.SubscribesDataAccess.Subscribe(update.CallbackQuery.Message.Chat.Id.ToString(),
                        update.CallbackQuery.Message.Chat.Username + " (" +
                        (update.CallbackQuery.Message.Chat.FirstName + " " + update.CallbackQuery.Message.Chat.LastName)
                        .Trim() + ")",
                        envs);
                    _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                        $"Select " + update.CallbackQuery.Data + ". " + $"You subscribe envs: " + envs.Replace(",", ", ") +
                        ".");
                }
            }
        }

        private void TaskListCallback(Update update)
        {
            string envs = null;

            var commands = new List<string> { "RunBuild", "RunDeploy", "Stop", "Status" };
            var lines = new List<List<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton>>();

            int j = 1;
            for (int i = 0; i < commands.Count; i++)
            {
                if (lines.Count < j)
                    lines.Add(new List<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton>());

                lines[j - 1].Add(Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton
                    .WithCallbackData(commands[i]));

                if (lines[j - 1].Count == 2) j++;
            }

            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(lines);

            _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Data+":",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Default,
                replyMarkup: keyboard);
        }

        private void ExtenvCallback(Update update)
        {
            string extenv = update.CallbackQuery.Message.Text.Replace(":", string.Empty);
            string command = update.CallbackQuery.Data;

            var id = Guid.NewGuid().ToString().Substring(0, 8);
            _dataAccess.ExternalCommandDataAccess.AddExternalCommand(id, update.CallbackQuery.Message.Chat.Id.ToString(), command, extenv);

            _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                $"Execute " + command + " for " + extenv+"...");
        }

        private void SubscribeEnvsCallback(Update update)
        {
            string envs = null;

            if (update.CallbackQuery.Data == "all")
                envs = update.CallbackQuery.Data;
            else
            {
                envs = _dataAccess.SubscribesDataAccess.GetEnvsByChatId(update.CallbackQuery.Message.Chat.Id.ToString());
                if (string.IsNullOrEmpty(envs))
                    envs = update.CallbackQuery.Data;
                else
                {
                    if (string.Equals(envs, "all", StringComparison.OrdinalIgnoreCase))
                        envs = update.CallbackQuery.Data;
                    else
                        envs = envs + "," + update.CallbackQuery.Data;
                }
            }

            _dataAccess.SubscribesDataAccess.Subscribe(update.CallbackQuery.Message.Chat.Id.ToString(),
                update.CallbackQuery.Message.Chat.Username + " (" +
                (update.CallbackQuery.Message.Chat.FirstName + " " + update.CallbackQuery.Message.Chat.LastName).Trim() +
                ")", envs);
            _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                $"Select " + update.CallbackQuery.Data + ". " + $"You subscribe envs: " + envs.Replace(",", ", ") + ".");
        }
    }
}
