using Newtonsoft.Json;

namespace ExtenvBot.Models
{
    [JsonObject()]
    public class Message
    {
        /// <summary>
        /// User's or bot's first name
        /// </summary>
        [JsonProperty(PropertyName = "env", Required = Required.Always)]
        public string Env { get; set; }

        [JsonProperty(PropertyName = "text", Required = Required.Always)]
        public string Text { get; set; }
    }
}
