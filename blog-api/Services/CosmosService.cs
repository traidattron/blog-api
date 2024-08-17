using blog_api.Data;
using Microsoft.Azure.Cosmos;

namespace blog_api.Services
{
    public class CosmosService : ICosmosService
    {
        private readonly string CosmosDbConnectionString = "AccountEndpoint=https://database-blog.documents.azure.com:443/;AccountKey=o4LgNsD6Ae31NKzxJISXtlvC337xSFnfjxFNxkTnuuYWe17oREsQksrEO3nLpBelVm9H6WzSHqdlACDb7pAMtA==;";
        private readonly string CosmosDbName = "blog";
        private readonly string CosmosDbContainerName = "Images";
        private Container GetContainerClient()
        {
            var cosmosDbClient = new CosmosClient(CosmosDbConnectionString,
                new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
                });
            var container = cosmosDbClient.GetContainer(CosmosDbName, CosmosDbContainerName);
            return container;
        }
        public async Task UpsertImage(Image image)
        {
            try
            {
                if (image.Id == null)
                {
                    image.Id = Guid.NewGuid().ToString();
                }
                var container = GetContainerClient();
                var updateRes = await container.UpsertItemAsync(image, new PartitionKey(image.Id));
                Console.Write(updateRes.StatusCode);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception", ex);
            }
        }
        public async Task DeleteImage(string? id, string? partitionKey)
        {
            try
            {
                var container = GetContainerClient();
                var response = await container.DeleteItemAsync<Image>(id, new PartitionKey(partitionKey));
            }
            catch (Exception ex)
            {
                throw new Exception("Exception", ex);
            }
        }
        public async Task<List<Image>> GetImageDetails()
        {
            List<Image> engineers = new List<Image>();
            try
            {
                var container = GetContainerClient();
                var sqlQuery = "SELECT * FROM c";
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<Image> queryResultSetIterator = container.GetItemQueryIterator<Image>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Image> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Image engineer in currentResultSet)
                    {
                        engineers.Add(engineer);
                    }
                }
            }
            catch (Exception ex)
            {

                ex.Message.ToString();
            }
            return engineers;
        }
        public async Task<Image> GetImageDetailsById(string? id, string? partitionKey)
        {
            try
            {
                var container = GetContainerClient();
                ItemResponse<Image> response = await container.ReadItemAsync<Image>(id, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (Exception ex)
            {

                throw new Exception("Exception ", ex);
            }
        }

    }
}
