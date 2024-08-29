using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AzureServiceBusDemo.Repositories;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace blog_appfunction_inprocess
{
    public class PushNotificationFunction
    {
        private readonly IServiceBus _serviceBus;
        public PushNotificationFunction(IServiceBus serviceBus)
        {
            this._serviceBus = serviceBus;
        }
        public async Task SendMessageAsync<Image>(Image messageDetail)
        {
            IQueueClient client = new QueueClient(Environment.GetEnvironmentVariable("AzureServiceBusConnectionString"), Environment.GetEnvironmentVariable("QueueNotificationName"));
            //Serialize car details object
            var messageBody = JsonSerializer.Serialize(messageDetail);
            //Set content type and Guid
            var message = new Message(Encoding.UTF8.GetBytes(messageBody))
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/json"
            };
            await client.SendAsync(message);
        }
        [FunctionName("PushNotificationFunction")]
        public void Run([ServiceBusTrigger("blog-topic-1", "blog-subcription-1", Connection = "topic_SERVICEBUS")]string mySbMsg, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg}");
            //connect to queue
            
            SendMessageAsync(mySbMsg).Wait();
            //push message to queue
        }
    }
}
