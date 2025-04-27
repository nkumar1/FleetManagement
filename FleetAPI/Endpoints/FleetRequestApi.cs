using FleetAPI.Models;
using FleetAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FleetAPI.Endpoints
{
    public class FleetRequestApi
    {
        private readonly KafkaProducerService _kafkaProducer;

        public FleetRequestApi(KafkaProducerService kafkaProducer)
        {
            _kafkaProducer = kafkaProducer;
        }

        public void RegisterRoutes(WebApplication app)
        {
            app.MapPost("/api/fleet/request", async ([FromBody] FleetRequestModel model) =>
            {
                var message = JsonConvert.SerializeObject(model);
                await _kafkaProducer.ProduceMessageAsync("fleet-requests-topic", message);
                return Results.Ok(new { message = "Fleet request submitted." });
            });
        }
    }
}
