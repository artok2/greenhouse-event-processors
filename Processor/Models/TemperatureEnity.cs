using Newtonsoft.Json;


namespace GreenHouse.Event.Processor.Models
{
                 
    public class TemperatureEntity
    {
        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("temperature")]
        public double Temperature { get; set; }

      
    }
}
