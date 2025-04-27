using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Fleet.Orchestrator
{
    public static class FleetRequestOrchestrator
    {
        [Function(nameof(FleetRequestOrchestrator))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(FleetRequestOrchestrator));
            logger.LogInformation("Inside RunOrchestrator");

            var input = context.GetInput<string>();
            var outputs = new List<string>();

            outputs.Add(await context.CallActivityAsync<string>(nameof(AssignVehicle), input));
            outputs.Add(await context.CallActivityAsync<string>(nameof(SendNotification), input));
           
            return outputs;
        }

        [Function(nameof(AssignVehicle))]
        public static string AssignVehicle([ActivityTrigger] string fleetRequest)
        {
            // Assign vehicle logic
            return $"Vehicle assigned: {fleetRequest}";
        }

        [Function(nameof(SendNotification))]
        public static string SendNotification([ActivityTrigger] string fleetRequest)
        {
            // Notification logic
            return $"Notification sent: {fleetRequest}";
        }

        // HTTP Starter Function - Needed in Isolated Worker Model [ context is of type TaskOrchestrationContext]

        [Function("StartFleetRequestOrchestration")]
        public static async Task<HttpResponseData> StartOrchestration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "start-orchestration")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("StartFleetRequestOrchestration");
            logger.LogInformation("Received a request to start FleetRequestOrchestrator.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            logger.LogInformation($"Request body: {requestBody}");

            // Start the orchestration
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(FleetRequestOrchestrator), requestBody);

            logger.LogInformation($"Started orchestration with ID: {instanceId}");

            var response = req.CreateResponse(System.Net.HttpStatusCode.Accepted);
            await response.WriteStringAsync($"Orchestration started. Instance ID: {instanceId}");

            return response;
        }
    }
}
