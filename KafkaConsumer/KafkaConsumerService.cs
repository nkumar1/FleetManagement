using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace KafkaConsumer
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _durableFunctionUrl;

        public KafkaConsumerService(
            ILogger<KafkaConsumerService> logger,
            IConfiguration config,
            IHttpClientFactory httpClientFactory
            )
        {
            _logger = logger;
            _config = config;

            _httpClientFactory = httpClientFactory;
            _durableFunctionUrl = _config["DurableFunction:OrchestrationStartUrl"]; // Load from config
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config["Kafka:BootstrapServers"],
                GroupId = "fleet-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            consumer.Subscribe("fleet-requests-topic");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    var message = consumeResult.Message.Value;

                    Console.WriteLine($"Consumed message: {consumeResult.Message.Value}");

                    // Trigger Durable Function
                    var client = _httpClientFactory.CreateClient();
                    var content = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(_durableFunctionUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Durable Function triggered successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"Durable Function trigger failed: {response.StatusCode}");
                    }
                }

                catch (ConsumeException ex)
                {
                    Console.WriteLine($"Kafka consume error: {ex.Error.Reason}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }
            }
        }
    }
}
