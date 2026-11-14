using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace TechMove.Tests.Integration;

public sealed class AgreementsServiceRatesTests(TechMoveApiFactory factory) : IClassFixture<TechMoveApiFactory>
{
    [Fact]
    public async Task UploadAgreement_PdfFile_Returns200ThenDownloadReturnsSameBytes()
    {
        using var client = await factory.CreateAdminClientAsync();
        var contractId = await TestDataBuilder.CreateContractWithStatusAsync(client, "Active");
        var expected = new byte[] { 37, 80, 68, 70, 45, 1, 2, 3 };
        using var upload = new MultipartFormDataContent();
        upload.Add(new ByteArrayContent(expected), "file", "agreement.pdf");
        var uploadResponse = await client.PostAsync($"api/contracts/{contractId}/agreement", upload);
        var downloadResponse = await client.GetAsync($"api/contracts/{contractId}/agreement");
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await downloadResponse.Content.ReadAsByteArrayAsync()).Should().Equal(expected);
    }

    [Fact]
    public async Task UploadAgreement_ExeFile_Returns400()
    {
        using var client = await factory.CreateAdminClientAsync();
        var contractId = await TestDataBuilder.CreateContractWithStatusAsync(client, "Active");
        using var upload = new MultipartFormDataContent();
        upload.Add(new ByteArrayContent(new byte[] { 1, 2 }), "file", "malware.exe");
        var response = await client.PostAsync($"api/contracts/{contractId}/agreement", upload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DownloadAgreement_NoneUploaded_Returns404()
    {
        using var client = await factory.CreateAdminClientAsync();
        var contractId = await TestDataBuilder.CreateContractWithStatusAsync(client, "Active");
        var response = await client.GetAsync($"api/contracts/{contractId}/agreement");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateServiceRequest_ActiveContract_Returns201AndZarCostEquals_UsdTimes18_50()
    {
        using var client = await factory.CreateAdminClientAsync();
        var contractId = await TestDataBuilder.CreateContractWithStatusAsync(client, "Active");
        var response = await client.PostAsJsonAsync("api/servicerequests", new { contractId, description = "Freight", amountUsd = 100m, status = "Pending" });
        var body = await response.Content.ReadFromJsonAsync<ServiceRequestResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        body!.CostZar.Should().Be(1850.00m);
        body.ExchangeRate.Should().Be(18.50m);
    }

    [Theory]
    [InlineData("Expired")]
    [InlineData("OnHold")]
    public async Task CreateServiceRequest_ExpiredOrOnHoldContract_Returns400(string status)
    {
        using var client = await factory.CreateAdminClientAsync();
        var contractId = await TestDataBuilder.CreateContractWithStatusAsync(client, status);
        var response = await client.PostAsJsonAsync("api/servicerequests", new { contractId, description = "Blocked", amountUsd = 100m, status = "Pending" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateServiceRequest_NegativeAmount_Returns400()
    {
        using var client = await factory.CreateAdminClientAsync();
        var contractId = await TestDataBuilder.CreateContractWithStatusAsync(client, "Active");
        var response = await client.PostAsJsonAsync("api/servicerequests", new { contractId, description = "Invalid", amountUsd = -1m, status = "Pending" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateThenGet_ServiceRequest_ReturnsSameData()
    {
        using var client = await factory.CreateAdminClientAsync();
        var contractId = await TestDataBuilder.CreateContractWithStatusAsync(client, "Active");
        var createResponse = await client.PostAsJsonAsync("api/servicerequests", new { contractId, description = "Tracked", amountUsd = 25m, status = "Pending" });
        var created = await createResponse.Content.ReadFromJsonAsync<ServiceRequestResponse>();
        var read = await client.GetFromJsonAsync<ServiceRequestResponse>($"api/servicerequests/{created!.Id}");
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        read!.ContractId.Should().Be(contractId);
        read.Description.Should().Be("Tracked");
        read.AmountUsd.Should().Be(25m);
        read.CostZar.Should().Be(462.50m);
    }

    [Fact]
    public async Task GetUsdZarRate_Returns200AndRate18_50()
    {
        using var client = await factory.CreateAdminClientAsync();
        var response = await client.GetAsync("api/exchangerates/usd-zar");
        var body = await response.Content.ReadFromJsonAsync<ExchangeRateResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Rate.Should().Be(18.50m);
    }

    private sealed record ServiceRequestResponse(int Id, int ContractId, string Description, decimal AmountUsd, decimal CostZar, decimal ExchangeRate, string Status);
    private sealed record ExchangeRateResponse(decimal Rate, DateTime FetchedAtUtc);
}
