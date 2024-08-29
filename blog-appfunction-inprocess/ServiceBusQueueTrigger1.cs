using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using AzureServiceBusDemo.Repositories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Company.Function
{
    public class ServiceBusQueueTrigger1
    {
        private readonly IServiceBus _serviceBus;
        public ServiceBusQueueTrigger1(IServiceBus serviceBus)
        {
            this._serviceBus = serviceBus;
        }
        public async Task TopicSendMessageAsync<Image>(Image messageDetail, ILogger ilogger)
        {
            ServiceBusClient client;
            ServiceBusSender sender;
            const int numOfMessages = 1;

            ilogger.LogInformation("AzureServiceBusKeyConnectionString: ",Environment.GetEnvironmentVariable("AzureServiceBusConnectionString"));
            ilogger.LogInformation("TopicName: ", Environment.GetEnvironmentVariable("TopicName"));
            client = new ServiceBusClient(Environment.GetEnvironmentVariable("AzureServiceBusConnectionString"));
            sender = client.CreateSender(Environment.GetEnvironmentVariable("TopicName"));
            ilogger.LogInformation("already connect to topic");

            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
            ilogger.LogInformation("already create batch");
            for (int i = 1; i <= numOfMessages; i++)
            {
                ilogger.LogInformation("in the loop create batch");
                // try adding a message to the batch
                if (!messageBatch.TryAddMessage(new ServiceBusMessage(JsonConvert.SerializeObject(messageDetail))))
                {
                    // if it is too large for the batch
                    throw new Exception($"The message {i} is too large to fit in the batch.");
                }
            }

            try
            {
                ilogger.LogInformation("sending message batch ...");
                // Use the producer client to send the batch of messages to the Service Bus topic
                await sender.SendMessagesAsync(messageBatch);
                ilogger.LogInformation("sent message batch ...");
                //Console.WriteLine($"A batch of {numOfMessages} messages has been published to the topic.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            //Console.WriteLine("Press any key to end the application");
            //Console.ReadKey();
        }
        private static IImageEncoder GetEncoder(string extension)
        {
            IImageEncoder encoder = null;
            extension = extension.Replace(".", "");
            var isSupported = Regex.IsMatch(extension, "gif|png|jpe?g", RegexOptions.IgnoreCase);
            if (isSupported)
            {
                switch (extension)
                {
                    case "png":
                        encoder = new PngEncoder();
                        break;
                    case "gif":
                        encoder = new GifEncoder();
                        break;
                    case "jpg":
                        encoder = new JpegEncoder();
                        break;
                    case "jpeg":
                        encoder = new JpegEncoder();
                        break;
                    default:
                        break;
                }
            }
            return encoder;
        }
        [FunctionName("ServiceBusQueueTrigger1")]
        public async Task Run([ServiceBusTrigger("blog-queue-1", Connection = "blogservicebus_SERVICEBUS")]string myQueueItem, 
            ILogger log)
        {

            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            var resizedInfo = JsonConvert.DeserializeObject<ImageResizeDto>(myQueueItem);

            log.LogInformation("Connect blob: ", Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var storageConn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var storageAcc = CloudStorageAccount.Parse(storageConn);
            var myClient = storageAcc.CreateCloudBlobClient();

            log.LogInformation("connect Container");
            var container = myClient.GetContainerReference("blog-container");
            
            log.LogInformation("Check container exist");
            await container.CreateIfNotExistsAsync();
            var blobName = resizedInfo.FileName;
            var cloudBlokBlob = container.GetBlobReference(blobName);
            var ms = new MemoryStream();
            
            log.LogInformation(cloudBlokBlob.Name);
            await cloudBlokBlob.DownloadToStreamAsync(ms);
            byte[] bytes = ms.ToArray();
            var extension = Path.GetExtension(resizedInfo.FileName);
            var encoder = GetEncoder(extension);
            using (var output = new MemoryStream())
            using(Image<Rgba32> image = Image.Load(bytes))
            {
                log.LogInformation("Image Resize has started");
                image.Mutate(x => x.Resize(new ResizeOptions()
                {
                    Size = new Size(resizedInfo.Width, resizedInfo.Height),
                    Compand = true,
                    Mode = ResizeMode.Max,
                }


                ));
                image.SaveAsync(output, encoder);
                output.Position = 0;

                var newFileName = $"resize_{resizedInfo.FileName}";
                var blobServiceClient = new BlobServiceClient(storageConn);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient("resizedimages");
                var blobCopy = container.GetBlobReference(newFileName);
                if (!await blobCopy.ExistsAsync())
                {
                    log.LogInformation("Upload to blob has started");
                    var uploadResut = await blobContainerClient.UploadBlobAsync(newFileName, output);
                    //log.LogInformation($"Result: {uploadResut.Value?.VersionId}");
                
                }

            };

            //send message to topic
            log.LogInformation($"Send message to topic");
           
            TopicSendMessageAsync(blobName, log).Wait();

        }
    }
}
