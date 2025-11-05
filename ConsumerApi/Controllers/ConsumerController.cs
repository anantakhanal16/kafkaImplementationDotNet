using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ProducerApi.OrderEvent;

namespace ConsumerApi.Controllers
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly string topic = "order-created-topic";

        public KafkaConsumerService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bootstrapServers =
                Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
                ?? configuration["Kafka:BootstrapServers"]
                ?? "localhost:9092"; // fallback for localhost

            // Step 1: Ensure topic exists before consuming
            await EnsureTopicExistsAsync(bootstrapServers, topic);

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = "payment-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    Console.WriteLine("Message received  " + cr.Message.Value);
                      var order = JsonSerializer.Deserialize<OrderCreatedEvent>(cr.Message.Value);

                    Console.WriteLine($"✅ PaymentService received OrderId: {order.OrderId}, Amount: {order.Amount}");

                    await Task.Delay(100, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }

        private async Task EnsureTopicExistsAsync(string bootstrapServers, string topicName)
        {
            var adminConfig = new AdminClientConfig { BootstrapServers = bootstrapServers };

            using var adminClient = new AdminClientBuilder(adminConfig).Build();

            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
                bool topicExists = metadata.Topics.Any(t => t.Topic == topicName && t.Error.Code == ErrorCode.NoError);

                if (!topicExists)
                {
                    Console.WriteLine($"⚙️ Topic '{topicName}' not found. Creating...");

                    await adminClient.CreateTopicsAsync(new[]
                    {
                        new TopicSpecification
                        {
                            Name = topicName,
                            NumPartitions = 3,
                            ReplicationFactor = 1
                        }
                    });

                    Console.WriteLine($"✅ Topic '{topicName}' created successfully.");
                }
                else
                {
                    Console.WriteLine($"✅ Topic '{topicName}' already exists.");
                }
            }
            catch (CreateTopicsException ex)
            {
                // If topic already exists, ignore error
                if (ex.Results.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
                {
                    Console.WriteLine($"ℹ️ Topic '{topicName}' already exists (ignored).");
                }
                else
                {
                    Console.WriteLine($"❌ Error creating topic '{topicName}': {ex.Results[0].Error.Reason}");
                }
            }
        }
    }
}
