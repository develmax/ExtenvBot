using Newtonsoft.Json;

namespace ExtenvBot.Models
{
    [JsonObject()]
    public class ExternalCommandResult
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "response", Required = Required.Always)]
        public string Response { get; set; }
    }
}