using blog_api.Services;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using AzureServiceBusDemo.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.
var storageConnectionString = builder.Configuration["AzureStorage:ConnectionString"];

builder.Services.AddAzureClients(builder =>
{
    builder.AddBlobServiceClient(storageConnectionString);
    
});

//builder.Services.AddAzureClients(b => {
//    b.AddClient<QueueClient, QueueClientOptions>((_, _, _) =>
//    {
//        return new QueueClient(storageConnectionString,
//                builder.Configuration["AzureStorage:QueueName"],
//                new QueueClientOptions
//                {
//                    MessageEncoding = QueueMessageEncoding.Base64
//                });
//    });

//    b.AddClient<TableClient, TableClientOptions>((_, _, _) =>
//    {
//        return new TableClient(storageConnectionString,
//                builder.Configuration["AzureStorage:TableStorage"]);
//    });
//});

builder.Services.AddScoped<IServiceBus, ServiceBus>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<ICosmosService, CosmosService>();
builder.Services.AddControllers();
builder.Services.AddCors();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200"));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
