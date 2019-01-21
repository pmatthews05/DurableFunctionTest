using System;
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
    public static class Activities
    {
        [FunctionName("delayedActivity")]
        public static async Task<int> delayedActivity(
            [ActivityTrigger]int number,
            ILogger log,
            ExecutionContext exCtx)
        {
            log.LogInformation($"Start Processing number {number} ");

            await Task.Delay(30000);

            log.LogInformation($"End Processing number {number} ");

            return number;
        }
        
        [FunctionName("whatsTheTime")]
        public static DateTime whatsTheTime (
         [ActivityTrigger]DurableActivityContext input,
            ILogger log,
            ExecutionContext exCtx)
        {
            return DateTime.Now;
        }

    }
}
