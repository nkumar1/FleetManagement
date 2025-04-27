
using FleetAPI.Endpoints;
using FleetAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<KafkaProducerService>();
builder.Services.AddSingleton<FleetRequestApi>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var fleetRequestApi = app.Services.GetRequiredService<FleetRequestApi>();
fleetRequestApi.RegisterRoutes(app);

app.Run();
