using Azure.Monitor.OpenTelemetry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI support
builder.Services.AddOpenApi(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable Azure Monitor OpenTelemetry
var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
builder.Services.AddOpenTelemetry()
   .UseAzureMonitor(options => 
   {
       options.ConnectionString = connectionString;
   });

var app = builder.Build();

// Serve OpenAPI UI
app.MapOpenApi();

// ✅ Update the SwaggerUI to point to the correct JSON location
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // Point to the .NET 9 OpenAPI document
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
    options.RoutePrefix = string.Empty; 
});

var products = new List<Product>();

// GET /products
app.MapGet("/products", () => products);

// POST /products
app.MapPost("/products", (Product product) => {
    products.Add(product);
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