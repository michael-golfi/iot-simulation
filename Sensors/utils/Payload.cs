using Newtonsoft.Json;

namespace IoTEdgeFridgeSimulator.Utils
{
    public class Payload
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "messageId")]
        public int MessageId { get; set; }

        [JsonProperty(PropertyName = "value")]
        public double Value { get; set; }
    }
}