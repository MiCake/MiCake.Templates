using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace StandardWeb.Web.StartUp;

public static class PollyHelper
{
    // Retry policy for general HTTP calls
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles HttpRequestException, 5XX and 408 responses
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Log.Warning("Retry {RetryCount} for {Context} in {Timespan}ms. Reason: {Reason}",
                        retryCount,
                        context.OperationKey ?? "Unknown",
                        timespan.TotalMilliseconds,
                        outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase ?? "Unknown");
                });
    }

    // Circuit breaker to prevent cascading failures
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    Log.Error("Circuit breaker opened for {Duration}ms. Reason: {Reason}",
                        duration.TotalMilliseconds,
                        exception.Exception?.Message ?? exception.Result?.ReasonPhrase ?? "Unknown");
                },
                onReset: () =>
                {
                    Log.Information("Circuit breaker reset");
                });
    }
}
