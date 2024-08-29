using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace blog_appfunction_inprocess
{
    public class NotificationHandleFunction
    {
        [FunctionName("NotificationHandleFunction")]
        public void Run([ServiceBusTrigger("notification-queue", Connection = "blogservicebus_SERVICEBUS")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
