using Azure.Storage.Queues;
using System.Text.Json;
using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI support 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable Azure Monitor OpenTelemetry
// var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
// builder.Services.AddOpenTelemetry()
//    .UseAzureMonitor(options => 
//    {
//        options.ConnectionString = connectionString;
//    });

var app = builder.Build();

// Serve OpenAPI UI
//app.MapOpenApi();
// Replace app.MapOpenApi() with:
if (app.Environment.IsDevelopment() || true) // 'true' helps us see it in Azure for now
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// ✅ Update the SwaggerUI to point to the correct JSON location
// app.UseSwagger();
// app.UseSwaggerUI(options =>
// {
//     // Point to the .NET 9 OpenAPI document
//     options.SwaggerEndpoint("/openapi/v1.json", "v1");
//     options.RoutePrefix = string.Empty; 
// });

var products = new List<Product>();

// GET /products
app.MapGet("/products", () => products);

// POST /products
app.MapPost("/products", async (Product product) => {
    products.Add(product);
    var connectionString = builder.Configuration.GetConnectionString("AzureWebJobsStorage") 
    ?? throw new InvalidOperationException("A connection string was not found...");
    var queueClient = new QueueClient(connectionString, "product-updates");
    await queueClient.CreateIfNotExistsAsync();
    if (await queueClient.ExistsAsync())
    {
        var message = JsonSerializer.Serialize(product);
        await queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message)));
    }

    return Results.Created($"/products/{product.Id}", product);
});

// PUT /products/{id}
app.MapPut("/products/{id}", (int id, Product updatedProduct) => {
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null) return Results.NotFound();
    
    product.Name = updatedProduct.Name;
    return Results.NoContent();
});

// DELETE /products/{id}
app.MapDelete("/products/{id}", (int id) => {
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null) return Results.NotFound();
    
    products.Remove(product);
    return Results.Ok();
});

// debug: list all endpoints (remove when done)
app.MapGet("/__routes", (EndpointDataSource ds) =>
{
    return Results.Ok(ds.Endpoints
        .Select(e => new { DisplayName = e.DisplayName, Pattern = (e as RouteEndpoint)?.RoutePattern?.RawText }));
});

app.Run();

// Product model
public class Product {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}