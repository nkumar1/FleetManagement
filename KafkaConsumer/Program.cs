using KafkaConsumer;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddHostedService<KafkaConsumerService>();

var host = builder.Build();
host.Run();
