
using System.Text.Json;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Azure.Messaging.ServiceBus;

namespace AzureServiceBusDemo.Repositories
{
    public class ServiceBus : IServiceBus
    {
        private readonly IConfiguration _configuration;
        public ServiceBus(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task RecieveMessageAsync(string messageDetail)
        {
            throw new NotImplementedException();
        }

        public async Task SendMessageAsync<Image>(Image messageDetail)
        {
            IQueueClient client = new QueueClient(_configuration["AzureServiceBusConnectionString"], _configuration["QueueName"]);
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
        public async Task TopicSendMessageAsync<Image>(Image messageDetail)
        {
            ServiceBusClient client;
            ServiceBusSender sender;
            const int numOfMessages = 1;
            client = new ServiceBusClient(_configuration["AzureServiceBusConnectionString"]);
            sender = client.CreateSender(_configuration["TopicName"]);
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            for (int i = 1; i <= numOfMessages; i++)
            {
                // try adding a message to the batch
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                {
                    // if it is too large for the batch
                    throw new Exception($"The message {i} is too large to fit in the batch.");
                }
            }

            try
            {
                // Use the producer client to send the batch of messages to the Service Bus topic
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {numOfMessages} messages has been published to the topic.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            Console.WriteLine("Press any key to end the application");
            Console.ReadKey();
        }
    }
}