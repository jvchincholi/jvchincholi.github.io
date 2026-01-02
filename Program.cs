using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Identity.Web; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI support 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable Azure Monitor OpenTelemetry
//var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
//builder.Services.AddOpenTelemetry()
//   .UseAzureMonitor(options => 
//   {
//      options.ConnectionString = connectionString;
//   });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.IncludeErrorDetails = true; // This will show the reason in the WWW-Authenticate header
        options.TokenValidationParameters.ValidateAudience = false;
        // This tells the API to accept both v1 and v2 issuer formats
        options.TokenValidationParameters.ValidIssuers = new[] 
        { 
            $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/v2.0",
            $"https://sts.windows.net/{builder.Configuration["AzureAd:TenantId"]}/" 
        };
    }, options => { builder.Configuration.Bind("AzureAd", options); });

// 2. Define the Authorization Rules (The "Permission")
builder.Services.AddAuthorization(options =>
{
    // We create a policy named "Products.Write"
    options.AddPolicy("Products.Write", policy => 
        // This policy requires the "scp" (scope) claim from Azure to contain "Products.Write"
        // We use "roles" (plural) because that is what Azure AD v2.0 sends
        policy.RequireClaim("roles", "Products.Write"));
});

builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type=ReferenceType.SecurityScheme, Id="Bearer" }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

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
app.MapGet("/products", [Authorize] async () => products);

// POST /products
app.MapPost("/products", [Authorize] async (Product product) => {
    products.Add(product);
    return Results.Created($"/products/{product.Id}", product);
}).RequireAuthorization("Products.Write");

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