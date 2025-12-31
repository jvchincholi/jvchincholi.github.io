using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BackgroundTasks
{
    public class ProcessProductQueue
    {
        private readonly ILogger<ProcessProductQueue> _logger;

        public ProcessProductQueue(ILogger<ProcessProductQueue> logger)
        {
            _logger = logger;
        }

        // This function wakes up ONLY when a message hits the "product-updates" queue
        [Function("ProcessProductQueue")]
        public void Run([QueueTrigger("product-updates", Connection = "AzureWebJobsStorage")] string message)
        {
            _logger.LogInformation("--- API MESSAGE RECEIVED ---");
            _logger.LogInformation("Processing product data: {message}", message);
            _logger.LogInformation("-----------------------------");
        }
    }
}