using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fintrellis.Services.Resiliency
{
    using Fintrellis.Services.Interfaces;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Retry;

    public class PollyRetryHandler : IRetryHandler
    {
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly ILogger<PollyRetryHandler> _logger;

        public PollyRetryHandler(int retryCount, int incrementalCount, ILogger<PollyRetryHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _retryPolicy = Policy
                .Handle<Exception>() 
                .WaitAndRetryAsync(
                    retryCount,
                    retryAttempt => TimeSpan.FromSeconds(retryAttempt * incrementalCount),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Retry {RetryCount} encountered an error: {Exception}. Retrying in {Delay}s...",
                            retryCount, exception.Message, timeSpan.TotalSeconds);
                    });
        }

        public async Task ExecuteWithRetryAsync(Func<Task> action)
        {
            await _retryPolicy.ExecuteAsync(action);
        }
    }
}
