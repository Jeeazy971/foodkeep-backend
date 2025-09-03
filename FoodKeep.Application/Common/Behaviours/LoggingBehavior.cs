using MediatR;
using Microsoft.Extensions.Logging;

namespace FoodKeep.Application.Common.Behaviours;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        _logger.LogInformation("FoodKeep Request: {Name} {@Request}", requestName, request);
        
        var response = await next();
        
        _logger.LogInformation("FoodKeep Response: {Name} {@Response}", requestName, response);
        
        return response;
    }
}
