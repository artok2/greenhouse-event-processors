using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace GreenHouse.Event.Processor.Models

{
    public class TelemetryGreenHouse
    {
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("cpuTemperature")]
        public double CPUTemperature { get; set; }

        [JsonPropertyName("altitude")]
        public double Altitude { get; set; }

        [JsonPropertyName("humidity")]
        public double Humidity { get; set; }

        [JsonPropertyName("pressure")]
        public double Pressure { get; set; }

        [JsonPropertyName("msgId")]
        public int MsgId { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("probability")]
        public double Probability { get; set; }

        [JsonIgnore]
        public string PartitionKey
        {
            get => $"{DeviceId}-{DateTime.UtcNow:yyyy-MM}";
        }

    }
}