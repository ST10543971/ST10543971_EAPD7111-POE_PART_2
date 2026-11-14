using System.Net.Http.Json;
using FluentAssertions;

namespace TechMove.Tests.Integration;

public static class TestDataBuilder
{
    public static async Task<(int ClientId, int ContractId)> CreateContractAsync(HttpClient client, string status = "Active", DateTime? startDate = null, DateTime? endDate = null)
    {
        var unique = Guid.NewGuid().ToString("N");
        var clientResponse = await client.PostAsJsonAsync("api/clients", new
        {
            name = $"Integration Client {unique}",
            contactDetails = "integration@test.local",
            region = "Test"
        });
        clientResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var createdClient = await clientResponse.Content.ReadFromJsonAsync<IdResponse>();

        var contractResponse = await client.PostAsJsonAsync("api/contracts", new
        {
            clientId = createdClient!.Id,
            startDate = (startDate ?? DateTime.UtcNow.Date).ToString("O"),
            endDate = (endDate ?? DateTime.UtcNow.Date.AddMonths(3)).ToString("O"),
            status,
            serviceLevel = "Integration Standard"
        });
        contractResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var createdContract = await contractResponse.Content.ReadFromJsonAsync<IdResponse>();
        return (createdClient.Id, createdContract!.Id);
    }

    public static async Task<int> CreateContractWithStatusAsync(HttpClient client, string status)
        => (await CreateContractAsync(client, status)).ContractId;

    private sealed record IdResponse(int Id);
}
