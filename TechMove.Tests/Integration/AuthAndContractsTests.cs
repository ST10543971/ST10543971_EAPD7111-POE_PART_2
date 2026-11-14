using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace TechMove.Tests.Integration;

public sealed class AuthAndContractsTests(TechMoveApiFactory factory) : IClassFixture<TechMoveApiFactory>
{
    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("api/auth/login", new { email = "admin@techmove.test", password = "Admin123!" });
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("api/auth/login", new { email = "admin@techmove.test", password = "wrong" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetContracts_WithoutToken_Returns401()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("api/contracts");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetContracts_WithToken_Returns200AndNonNullList()
    {
        using var client = await factory.CreateAdminClientAsync();
        var response = await client.GetAsync("api/contracts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateContract_ValidPayload_Returns201WithLocationHeader()
    {
        using var client = await factory.CreateAdminClientAsync();
        var (_, contractId) = await TestDataBuilder.CreateContractAsync(client);
        contractId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateThenGet_Contract_ReturnsSameData()
    {
        using var client = await factory.CreateAdminClientAsync();
        var unique = Guid.NewGuid().ToString("N");
        var clientResponse = await client.PostAsJsonAsync("api/clients", new { name = unique, contactDetails = "x", region = "Test" });
        var createdClient = await clientResponse.Content.ReadFromJsonAsync<IdResponse>();
        var start = DateTime.UtcNow.Date;
        var end = start.AddMonths(6);
        var createResponse = await client.PostAsJsonAsync("api/contracts", new { clientId = createdClient!.Id, startDate = start, endDate = end, status = "Active", serviceLevel = "Premium" });
        var location = createResponse.Headers.Location;
        var created = await createResponse.Content.ReadFromJsonAsync<ContractResponse>();
        var read = await client.GetFromJsonAsync<ContractResponse>(location!.ToString());
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        read!.ClientId.Should().Be(created!.ClientId);
        read.StartDate.Date.Should().Be(start);
        read.EndDate.Date.Should().Be(end);
        read.Status.Should().Be("Active");
        read.ServiceLevel.Should().Be("Premium");
    }

    [Fact]
    public async Task CreateContract_EndDateBeforeStartDate_Returns400()
    {
        using var client = await factory.CreateAdminClientAsync();
        var (clientId, _) = await TestDataBuilder.CreateContractAsync(client);
        var response = await client.PostAsJsonAsync("api/contracts", new { clientId, startDate = DateTime.UtcNow.Date.AddDays(2), endDate = DateTime.UtcNow.Date, status = "Draft", serviceLevel = "Invalid" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetContract_UnknownId_Returns404()
    {
        using var client = await factory.CreateAdminClientAsync();
        var response = await client.GetAsync("api/contracts/999999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetContracts_FilterByStatus_ReturnsOnlyThatStatus()
    {
        using var client = await factory.CreateAdminClientAsync();
        await TestDataBuilder.CreateContractAsync(client, "Draft");
        await TestDataBuilder.CreateContractAsync(client, "Active");
        var contracts = await client.GetFromJsonAsync<List<ContractResponse>>("api/contracts?status=Active");
        contracts.Should().NotBeNullOrEmpty();
        contracts!.Should().OnlyContain(x => x.Status == "Active");
    }

    [Fact]
    public async Task GetContracts_FilterByDateRange_ReturnsOnlyContractsInRange()
    {
        using var client = await factory.CreateAdminClientAsync();
        var start = DateTime.UtcNow.Date.AddDays(10);
        await TestDataBuilder.CreateContractAsync(client, "Active", start, start.AddDays(2));
        var contracts = await client.GetFromJsonAsync<List<ContractResponse>>($"api/contracts?startFrom={start:O}&endTo={start.AddDays(2):O}");
        contracts.Should().NotBeNullOrEmpty();
        contracts!.Should().OnlyContain(x => x.StartDate >= start && x.EndDate <= start.AddDays(2));
    }

    [Fact]
    public async Task PatchStatus_DraftToActive_AsAdmin_Returns2xxAndPersists()
    {
        using var client = await factory.CreateAdminClientAsync();
        var contractId = await TestDataBuilder.CreateContractWithStatusAsync(client, "Draft");
        var patch = await client.PatchAsJsonAsync($"api/contracts/{contractId}/status", new { status = "Active" });
        var read = await client.GetFromJsonAsync<ContractResponse>($"api/contracts/{contractId}");
        patch.IsSuccessStatusCode.Should().BeTrue();
        read!.Status.Should().Be("Active");
    }

    [Fact]
    public async Task PatchStatus_AsManager_Returns403()
    {
        using var admin = await factory.CreateAdminClientAsync();
        var contractId = await TestDataBuilder.CreateContractWithStatusAsync(admin, "Draft");
        using var manager = await factory.CreateManagerClientAsync();
        var response = await manager.PatchAsJsonAsync($"api/contracts/{contractId}/status", new { status = "Active" });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PatchStatus_InvalidTransition_Returns400()
    {
        using var client = await factory.CreateAdminClientAsync();
        var contractId = await TestDataBuilder.CreateContractWithStatusAsync(client, "Expired");
        var response = await client.PatchAsJsonAsync($"api/contracts/{contractId}/status", new { status = "Active" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record LoginResponse(string Token, DateTime ExpiresAtUtc, string Role, string Email);
    private sealed record IdResponse(int Id);
    private sealed record ContractResponse(int Id, int ClientId, string? ClientName, DateTime StartDate, DateTime EndDate, string Status, string ServiceLevel, string? SignedAgreementPath);
}
