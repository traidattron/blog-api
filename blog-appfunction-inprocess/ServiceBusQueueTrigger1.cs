using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
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
        public static async Task Run([ServiceBusTrigger("blog-queue-1", Connection = "blogservicebus_SERVICEBUS")]string myQueueItem, 
            ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            var resizedInfo = JsonConvert.DeserializeObject<ImageResizeDto>(myQueueItem);
            log.LogInformation("Connect blob");
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
                    log.LogInformation($"Result: {uploadResut.Value.VersionId}");
                }

            }
            
        }
    }
}
