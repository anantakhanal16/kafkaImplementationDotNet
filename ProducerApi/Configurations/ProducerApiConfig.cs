namespace ProducerApi.Configurations
{
    public class ProducerApiConfig
    {
        public class KafkaSettings
        {
            public string BootstrapServers { get; set; }
            public KafkaTopics Topics { get; set; }
        }

        public class KafkaTopics
        {
            public string OrderCreated { get; set; }
        }
    }
}
