using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MyCompany.Functions
{
    public class ProductFunction
    {
        private readonly ILogger<ProductFunction> _logger;
        // In a real app, this would be a Database Context
        private static List<Product> _products = new List<Product>();

        public ProductFunction(ILogger<ProductFunction> logger)
        {
            _logger = logger;
        }

        // 1. GET ALL PRODUCTS (Requires any valid JWT)
        [Function("GetProducts")]
        [Authorize] 
        public async Task<IActionResult> GetProducts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req)
        {
            _logger.LogInformation("Fetching product list.");
            return new OkObjectResult(_products);
        }

        // 2. CREATE PRODUCT (Requires "Products.Write" Policy)
        [Function("CreateProduct")]
        [Authorize(Policy = "Products.Write")] 
        public async Task<IActionResult> CreateProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequest req)
        {
            try {
            _logger.LogInformation("Creating a new product.");
            
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var product = JsonSerializer.Deserialize<Product>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (product == null) return new BadRequestResult();

            _products.Add(product);
            return new CreatedResult($"/api/products/{product.Id}", product);
            } 
            catch (Exception ex) {
                _logger.LogError($"CRASH: {ex.Message} - {ex.StackTrace}");
            return new ObjectResult(new { error = ex.Message }) { StatusCode = 500 };
            }           
        }

        // 3. DELETE PRODUCT (Example of route parameters)
        [Function("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{id}")] HttpRequest req, int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return new NotFoundResult();

            _products.Remove(product);
            return new OkResult();
        }
    }

    public class Product {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}