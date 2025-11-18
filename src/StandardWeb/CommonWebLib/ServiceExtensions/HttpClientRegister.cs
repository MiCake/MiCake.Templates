using Microsoft.Extensions.Configuration;
using StandardWeb.Application.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace CommonWebLib.ServiceExtensions;

public static class HttpClientRegister
{
    public static void RegisterHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddHttpClient(HttpClientNames.Default, client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(PollyHelper.GetRetryPolicy())
        .AddPolicyHandler(PollyHelper.GetCircuitBreakerPolicy());

        // Additional HttpClient registrations can be added here
    }
}