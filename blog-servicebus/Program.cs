// See https://aka.ms/new-console-template for more information

using Azure.Messaging.ServiceBus;


const string connectionString = "";
const string queueName = "";
const int maxNumberOfMessages = 3;

ServiceBusClient client;
ServiceBusSender sender;

client = new ServiceBusClient(connectionString);
sender = client.CreateSender(queueName);

using ServiceBusMessageBatch batch = await sender.CreateMessageBatchAsync();
for (int i = 1; i <= maxNumberOfMessages; i++)
{
    if(!batch.TryAddMessage(new ServiceBusMessage($"message {i}")))
    {
        Console.WriteLine($"message {i} is not added");
    }
}
try
{
    await sender.SendMessagesAsync(batch);
    Console.WriteLine("Message sent");

}
catch
{
    Console.WriteLine("Message sent");
    throw;
}
finally
{
    await sender.DisposeAsync();
    await client.DisposeAsync();
}
