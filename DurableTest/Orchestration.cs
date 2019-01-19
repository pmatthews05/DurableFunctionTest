using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableTest
{
    public static class Orchestration
    {
        [FunctionName("RunTest")]
        public static async Task<int> RunTest(
            [OrchestrationTrigger] DurableOrchestrationContextBase runTestContext, ILogger log, ExecutionContext exCtx)
        {
            var processTestTasks = new List<Task<int>>();
            var startTime = runTestContext.CallActivityAsync<DateTime>("whatsTheTime", null);

            if (!runTestContext.IsReplaying)
            {
                log.LogInformation($"Setting up activities to run time: {startTime}");
            }

            for (int i = 0; i < 400; i++)
            {
                Task<int> processTest = runTestContext.CallActivityAsync<int>("delayedActivity", i);
                processTestTasks.Add(processTest);
            }

            await Task.WhenAll(processTestTasks);

            var endTime = runTestContext.CallActivityAsync<DateTime>("whatsTheTime", null); 
            var totalTime = endTime.Result.Subtract(startTime.Result).TotalMinutes;

            int total = processTestTasks.Sum(t => t.Result);

            if (!runTestContext.IsReplaying)
            {
              log.LogInformation($"Total Minutes: {totalTime} ");
            }
            
            runTestContext.SetCustomStatus($"Total Minutes: {totalTime}");
            return total;
        }
    }
}
