
using System.Text.Json;
using System.Text;
using Microsoft.Azure.ServiceBus;
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
            IQueueClient client = new QueueClient(_configuration["AzureServiceBusConnectionString"], _configuration["TopicName"]);
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
    }
}