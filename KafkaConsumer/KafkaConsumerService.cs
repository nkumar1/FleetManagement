using Confluent.Kafka;

namespace KafkaConsumer
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConfiguration _config;

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
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
                var result = consumer.Consume(stoppingToken);
                Console.WriteLine($"Consumed message: {result.Message.Value}");

                // (Here you can call Durable Function HTTP Trigger)
            }
        }
    }
}
