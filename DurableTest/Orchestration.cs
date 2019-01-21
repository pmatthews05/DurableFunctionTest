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
            
            var startTime = await runTestContext.CallActivityAsync<DateTime>("whatsTheTime", null);

            if (!runTestContext.IsReplaying)
            {
                log.LogInformation($"Setting up activities to run time: {startTime}");
            }

            List<int> collectionOfInt = new List<int>();
            for (int i = 0; i < 400; i++)
            {
                collectionOfInt.Add(i);
            }

            #region Allow groups of 50
            int total = 0;
            var groupby50 = collectionOfInt.GroupElements(50);
            foreach(var intValues in groupby50)
            {
                var processTestTasks = new List<Task<int>>();
                //This is processing 50 at a time.
                foreach(var intValue in intValues)
                {
                    Task<int> processTest = runTestContext.CallActivityAsync<int>("delayedActivity", intValue);
                    processTestTasks.Add(processTest);
                    if (!runTestContext.IsReplaying)
                    {
                        log.LogInformation($"Processing {intValue}");
                    }
                }

                if (!runTestContext.IsReplaying)
                {
                    log.LogInformation("Calling When All");
                }
                await Task.WhenAll(processTestTasks);

                total += processTestTasks.Sum(t => t.Result);

                if (!runTestContext.IsReplaying)
                {
                    log.LogInformation($"Total value so far {total} ");
                }
            }
            #endregion

            var endTime = await runTestContext.CallActivityAsync<DateTime>("whatsTheTime", null);
            double totalTime = 0;
            
            try
            {
               totalTime = endTime.Subtract(startTime).TotalMinutes;
            }
            catch(Exception ex)
            {
                log.LogError("There has been an error" + ex.Message);
            }
            
            if (!runTestContext.IsReplaying)
            {
              log.LogInformation($"Total Minutes: {totalTime} ");
            }
            
            runTestContext.SetCustomStatus($"Total Minutes: {totalTime}");
            return total;
        }


        public static IEnumerable<IEnumerable<T>> GroupElements<T>(this List<T> fullList, int batchSize)
        {
            int total = 0;
            while (total < fullList.Count)
            {
                yield return fullList.Skip(total).Take(batchSize);
                total += batchSize;
            }
        }
    }
}
