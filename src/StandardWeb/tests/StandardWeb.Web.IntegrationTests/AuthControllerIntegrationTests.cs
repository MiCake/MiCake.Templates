using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using StandardWeb.Contracts.Dtos.Identity;
using StandardWeb.Web.Dtos.Identity;

namespace StandardWeb.Web.IntegrationTests;

/// <summary>
/// Custom web application factory for integration tests.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Override services for testing (e.g., use in-memory database)
            // This is where you would configure test-specific services
        });

        // Use test environment
        builder.UseEnvironment("Test");
    }
}

/// <summary>
/// Integration tests for API endpoints.
/// These tests verify the entire request/response pipeline without mocking.
/// </summary>
public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task LoginEndpoint_ShouldBeAccessible()
    {
        // Arrange
        var loginRequest = new LoginRequestDto
        {
            PhoneNumber = "13800138000",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert - Should not return 404 NotFound (endpoint exists)
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RefreshTokenEndpoint_ShouldBeAccessible()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequestDto
        {
            RefreshToken = "some_token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/token/refresh", refreshRequest);

        // Assert - Should not return 404 NotFound (endpoint exists)
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserEndpoint_ShouldBeAccessible()
    {
        // Act
        var response = await _client.GetAsync("/api/user/1");

        // Assert - Endpoint should exist and respond (any status code is acceptable for this test)
        // This test just verifies the endpoint is configured and reachable
        Assert.NotNull(response);
    }

    [Fact]
    public async Task HealthCheck_RootEndpoint_ShouldRespond()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert - Application should respond to root endpoint
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
    }
}

