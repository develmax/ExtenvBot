using Newtonsoft.Json;

namespace ExtenvBot.Models
{
    [JsonObject()]
    public class ExternalCommand
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "command", Required = Required.Always)]
        public string Command { get; set; }

        [JsonProperty(PropertyName = "request", Required = Required.Always)]
        public string Request { get; set; }
    }
}