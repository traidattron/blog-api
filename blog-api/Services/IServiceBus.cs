
using blog_api.Services;
using System;
namespace AzureServiceBusDemo.Repositories
{
    public interface IServiceBus
    {
        Task SendMessageAsync<T>(T messageDetail);
        Task RecieveMessageAsync(string messageDetail);
    }
}