using Swashbuckle.AspNetCore.Filters;

namespace FoodKeep.API.SwaggerExamples;

public class HealthResponseExample : IExamplesProvider<object>
{
    public object GetExamples()
    {
        return new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "FoodKeep.API",
            version = "1.0.0",
            uptime = "00:45:23",
            dependencies = new[]
            {
                new { service = "PostgreSQL", status = "Healthy" },
                new { service = "Redis", status = "Healthy" },
                new { service = "RabbitMQ", status = "Healthy" }
            }
        };
    }
}

public class ErrorResponseExample : IExamplesProvider<object>
{
    public object GetExamples()
    {
        return new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Bad Request",
            status = 400,
            detail = "The request contains invalid data",
            instance = "/api/foods/123",
            traceId = "00-1234567890abcdef-1234567890abcdef-00",
            errors = new Dictionary<string, string[]>
            {
                ["name"] = new[] { "Name is required", "Name must be less than 100 characters" },
                ["expirationDate"] = new[] { "Expiration date must be in the future" }
            }
        };
    }
}
