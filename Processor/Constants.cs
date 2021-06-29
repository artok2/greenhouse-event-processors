namespace GreenHouse.Event.Processor
{
    public class Constants
    {
        public const string COSMOS_DB_DATABASE_NAME = "devices";
        public const string COSMOS_DB_CONTAINER_NAME = "greenhouse";
        public const string EVENT_HUB_TELEMETRY_GREENHOUSE= "raspberrypi";
        public const string EVENT_HUB_CONSUMER_GROUP = "function-stream-processor";
    }
}