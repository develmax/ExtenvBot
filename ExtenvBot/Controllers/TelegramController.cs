using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ExtenvBot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Telegram.Bot;
using TelegramBotApi;
using TelegramBotApi.Models;
using TelegramBotApi.Configurations;

namespace ExtenvBot.Controllers
{
    [Route("api/[controller]")]
    public class TelegramController : Controller
    {
        //ITelegramClient _telegramClient;
        ITelegramBotClient _bot;
        private IStorage _storage;
        private IConfiguration _configuration;
        public TelegramController(/*ITelegramClient telegramClient*/ ITelegramBotClient bot, IStorage storage, IConfiguration configuration) {
            //_telegramClient = telegramClient;
            _bot = bot;
            _storage = storage;
            _configuration = configuration;
        }

        // GET api/telegram/update/{token}
        [HttpGet("subscribes")]
        public IActionResult Subscribes()
        {
            var subscriptions = _storage.GetSubscriptions();

            return View(new SubscriptionsViewModel { EntityList = subscriptions != null ? subscriptions.ToList() : new List<SubscriptionEntity>() });
        }

        // GET api/telegram/update/{token}
        [HttpPost("update/{token}")]
        public void Update([FromRoute] string token, [FromBody] Telegram.Bot.Types.Update /*Update*/ update)
        {
            /*if (token != _telegramClient.GetConfiguration().AuthenticationToken)
                return;*/
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
                        if (string.Equals(update.CallbackQuery.Message.Text, "subscribe envs:", StringComparison.OrdinalIgnoreCase))
                        {
                            /*_bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                $"Select " + update.CallbackQuery.Data + ".");*/

                            string envs = null;

                            if (update.CallbackQuery.Data == "all")
                                envs = update.CallbackQuery.Data;
                            else
                            {
                                envs = _storage.GetEnvs(update.CallbackQuery.Message.Chat.Id.ToString());
                                if (string.IsNullOrEmpty(envs))
                                    envs = update.CallbackQuery.Data;
                                else
                                    envs = envs + "," + update.CallbackQuery.Data;
                            }

                            _storage.Subscribe(update.CallbackQuery.Message.Chat.Id.ToString(),
                                update.CallbackQuery.Message.Chat.Username + " (" +
                                (update.CallbackQuery.Message.Chat.FirstName + " " + update.CallbackQuery.Message.Chat.LastName).Trim()+")", envs);
                            _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                $"Select " + update.CallbackQuery.Data + ". " + $"You subscribe envs: " + envs.Replace(",", ", ") + ".");
                        }
                        else if (string.Equals(update.CallbackQuery.Message.Text, "unsubscribe envs:", StringComparison.OrdinalIgnoreCase))
                        {
                            /*_bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                $"Select " + update.CallbackQuery.Data + ".");*/

                            string envs = null;

                            if (update.CallbackQuery.Data == "all")
                            {
                                _storage.UnSubscribe(update.CallbackQuery.Message.Chat.Id.ToString());
                                _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                    $"Select " + update.CallbackQuery.Data + ". " + $"I unsubscribe you.");
                            }
                            else
                            {
                                envs = _storage.GetEnvs(update.CallbackQuery.Message.Chat.Id.ToString());
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
                                    _storage.UnSubscribe(update.CallbackQuery.Message.Chat.Id.ToString());
                                    _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                        $"Select " + update.CallbackQuery.Data + ". " + $"I unsubscribe you.");
                                }
                                else
                                {
                                    _storage.Subscribe(update.CallbackQuery.Message.Chat.Id.ToString(),
                                        update.CallbackQuery.Message.Chat.Username + " (" +
                                        (update.CallbackQuery.Message.Chat.FirstName + " " + update.CallbackQuery.Message.Chat.LastName).Trim() + ")", 
                                        envs);
                                    _bot.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id,
                                        $"Select " + update.CallbackQuery.Data + ". " + $"You subscribe envs: " + envs.Replace(",", ", ") + ".");
                                }
                            }
                        }
                    }
                }
                else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    if (update.Message != null && !string.IsNullOrEmpty(update.Message.Text))
                    {

                        if (update.Message.Text.Equals("/start", StringComparison.OrdinalIgnoreCase))
                        {
                            /*_telegramClient.SendMessage(update.Message.Chat.Id,
                                $"Hello! I am TeamCity. Welcome." + Environment.NewLine +
                                "Use:"+Environment.NewLine +
                                "/subscribe - Subscribe on all env updates." + Environment.NewLine +
                                "/unsubscribe - Unsubscribe on all env updates." + Environment.NewLine +
                                "/list - List subscribe envs.");*/

                            _bot.SendTextMessageAsync(update.Message.Chat.Id,
                                $"Hello! I am TeamCity. Welcome!" + Environment.NewLine + Environment.NewLine +
                                "Commands:" + Environment.NewLine +
                                "/subscribe - Subscribe on all env updates." + Environment.NewLine +
                                "/unsubscribe - Unsubscribe on all env updates." + Environment.NewLine +
                                "/list - List subscribe envs." + Environment.NewLine + Environment.NewLine +
                                "\U0001F609" + " Write your features in this chat.");
                        }
                        else if (update.Message.Text.Equals("/subscribe", StringComparison.OrdinalIgnoreCase) ||
                                 update.Message.Text.StartsWith("/subscribe", StringComparison.OrdinalIgnoreCase))
                        {
                            var envs = _storage.GetEnvs(update.Message.Chat.Id.ToString());

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

                                    var text = name;// == "preprod" ? "\U0001F699" + name : "\U0001F697" + name;

                                    lines[j - 1].Add(Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton
                                        .WithCallbackData(text, name));

                                    if (lines[j - 1].Count == 5) j++;
                                }
                            }

                            //_bot.SendTextMessageAsync()
                            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(lines);

                            _bot.SendTextMessageAsync(update.Message.Chat.Id, "Subscribe envs:",
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Default,
                                replyMarkup: keyboard);

                            /*string envs;
    
                            if (string.Equals(update.Message.Text.Trim(), "/subscribe", StringComparison.OrdinalIgnoreCase))
                                envs = "all";
                            else
                                envs = update.Message.Text.Substring("/subscribe ".Length).Trim();
    
                            _storage.Subscribe(update.Message.Chat.Id.ToString(), envs);
    
                            //_telegramClient.SendMessage(update.Message.Chat.Id, $"I subscribe you on envs: \"{envs}\".");
                            _bot.SendTextMessageAsync(update.Message.Chat.Id, $"I subscribe you on envs: \"{envs}\".");*/
                        }
                        else if (update.Message.Text.Equals("/unsubscribe", StringComparison.OrdinalIgnoreCase))
                        {
                            var envs = _storage.GetEnvs(update.Message.Chat.Id.ToString());

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

                                    if (lines[j - 1].Count == 5) j++;
                                }
                            }

                            //_bot.SendTextMessageAsync()
                            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(lines);

                            _bot.SendTextMessageAsync(update.Message.Chat.Id, "Unsubscribe envs:",
                                parseMode: Telegram.Bot.Types.Enums.ParseMode.Default,
                                replyMarkup: keyboard);
                        }
                        else if(update.Message.Text.Equals("/stop", StringComparison.OrdinalIgnoreCase))
                        {
                            _storage.UnSubscribe(update.Message.Chat.Id.ToString());

                            //_telegramClient.SendMessage(update.Message.Chat.Id, $"I unsubscribe you.");
                            _bot.SendTextMessageAsync(update.Message.Chat.Id, $"I unsubscribe you.");
                            _bot.SendTextMessageAsync(update.Message.Chat.Id, $"Bye. I will see you later.");
                                //_telegramClient.SendMessage(update.Message.Chat.Id, $"Bye. I will see you later.");
                                
                        }
                        else if (update.Message.Text.Equals("/list", StringComparison.OrdinalIgnoreCase))
                        {
                            var envs = _storage.GetEnvs(update.Message.Chat.Id.ToString());

                            var text = string.IsNullOrEmpty(envs)
                                ? "Subscribe envs is not found."
                                : "Subscribe envs: " + envs.Replace(",", ", ") + ".";
                            //_telegramClient.SendMessage(update.Message.Chat.Id, text);
                            _bot.SendTextMessageAsync(update.Message.Chat.Id, text);
                        }
                        else if (update.Message.Text.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
                        {
                            _storage.AdminId = update.Message.Chat.Id;
                            _storage.AdminName = update.Message.Chat.Username;
                            //_telegramClient.SendMessage(update.Message.Chat.Id, $"Set admin is " + update.Message.Chat.Username + ".");
                            _bot.SendTextMessageAsync(update.Message.Chat.Id,
                                $"Set admin is " + update.Message.Chat.Username + ".");
                        }
                        else if (_storage.AdminId.HasValue)
                        {
                            //_telegramClient.SendMessage(update.Message.Chat.Id, $"Your text translate to TeamCity.");
                            _bot.SendTextMessageAsync(update.Message.Chat.Id, $"Your text translate to TeamCity.");
                            /*_telegramClient.SendMessage(_storage.AdminId.Value,
                                $"Query text by user " + update.Message.Chat.Username + "(" + (update.Message.Chat.FirstName + " " + update.Message.Chat.LastName).Trim() + ")" + ": " + update.Message.Text);*/

                            _bot.SendTextMessageAsync(_storage.AdminId.Value,
                                $"Query text by user " + update.Message.Chat.Username + " (" +
                                (update.Message.Chat.FirstName + " " + update.Message.Chat.LastName).Trim() + ")" +
                                ": " + update.Message.Text);
                        }

                    }
                }

            }
            catch (Exception e)
            {
                /*_telegramClient.SendMessage(update.Message.Chat.Id,
                    $"Exception: {e.ToString()}");*/
                _bot.SendTextMessageAsync(update.Message.Chat.Id,
                    $"Exception: {e.ToString()}");
                //throw;
            }
        }

    }
}
