using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Fleet.Orchestrator
{
    public static class Function1
    {
        [Function(nameof(Function1))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(Function1));
            logger.LogInformation("Saying hello.");

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

        //[Function(nameof(SayHello))]
        //public static string SayHello([ActivityTrigger] string name, FunctionContext executionContext)
        //{
        //    ILogger logger = executionContext.GetLogger("SayHello");
        //    logger.LogInformation("Saying hello to {name}.", name);
        //    return $"Hello {name}!";
        //}

        //[Function("Function1_HttpStart")]
        //public static async Task<HttpResponseData> HttpStart(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
        //    [DurableClient] DurableTaskClient client,
        //    FunctionContext executionContext)
        //{
        //    ILogger logger = executionContext.GetLogger("Function1_HttpStart");

        //    // Function input comes from the request content.
        //    string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
        //        nameof(Function1));

        //    logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        //    // Returns an HTTP 202 response with an instance management payload.
        //    // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        //    return await client.CreateCheckStatusResponseAsync(req, instanceId);
        //}
    }
}
