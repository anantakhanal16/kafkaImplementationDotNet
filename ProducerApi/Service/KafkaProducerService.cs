using Confluent.Kafka;
using System.Text.Json;

namespace ProducerApi.Service
{
    public class KafkaProducerService
    {
        private readonly ProducerConfig config;
        private readonly string topic = "order-created-topic";

        public KafkaProducerService(IConfiguration configuration)
        {
            config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
            };
        }

        public async Task<dynamic> PublishAsync<T>(T message)
        {
            using var producer = new ProducerBuilder<Null, string>(config).Build();
            var json = JsonSerializer.Serialize(message);
            var result = await producer.ProduceAsync(topic, new Message<Null, string> { Value = json });
            return result;
        }
    }

}
