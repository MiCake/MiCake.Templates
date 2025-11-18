using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace CommonWebLib.ServiceExtensions;

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

    public static IAsyncPolicy<HttpResponseMessage> GetStockDataRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Handle rate limiting
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt =>
                {
                    // Handle rate limiting with longer delays
                    if (retryAttempt <= 2)
                        return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    else
                        return TimeSpan.FromMinutes(retryAttempt - 1); // Longer delays for rate limiting
                },
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var statusCode = outcome.Result?.StatusCode.ToString() ?? "Exception";
                    Log.Warning("Stock data retry {RetryCount} in {Timespan}ms. Status: {StatusCode}. Reason: {Reason}",
                        retryCount,
                        timespan.TotalMilliseconds,
                        statusCode,
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
