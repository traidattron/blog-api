using blog_api.Data;

namespace blog_api.Services
{
    public interface ICosmosService
    {
        Task DeleteImage(string? id, string? partitionKey);
        Task<List<Image>> GetImageDetails();
        Task<Image> GetImageDetailsById(string? id, string? partitionKey);
        Task UpsertImage(Image image);
    }
}
