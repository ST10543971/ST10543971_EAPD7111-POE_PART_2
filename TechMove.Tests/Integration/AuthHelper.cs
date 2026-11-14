using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace TechMove.Tests.Integration;

public static class AuthHelper
{
    public static Task<HttpClient> CreateAdminClientAsync(this TechMoveApiFactory factory) => CreateAuthenticatedClientAsync(factory, "admin@techmove.test", "Admin123!");
    public static Task<HttpClient> CreateManagerClientAsync(this TechMoveApiFactory factory) => CreateAuthenticatedClientAsync(factory, "manager@techmove.test", "Manager123!");

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(TechMoveApiFactory factory, string email, string password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        return client;
    }

    private sealed record LoginResponse(string Token, DateTime ExpiresAtUtc, string Role, string Email);
}
