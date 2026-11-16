using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using TechMove.Models;

namespace TechMove.Services.ApiClients;

public sealed record ApiResult<T>(bool IsSuccess, T? Value, string? ErrorMessage, int StatusCode)
{
    public static ApiResult<T> Success(T value, int statusCode = 200) => new(true, value, null, statusCode);
    public static ApiResult<T> Failure(string message, int statusCode = 0) => new(false, default, message, statusCode);
}

public sealed record LoginApiResponse(string Token, DateTime ExpiresAtUtc, string Role, string Email);
public sealed record ExchangeRateApiResponse(decimal Rate, DateTime FetchedAtUtc);
public sealed record DownloadedFile(Stream Stream, string FileName, HttpResponseMessage Response);

public interface IClientApiService
{
    Task<ApiResult<IReadOnlyList<Client>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResult<Client>> CreateAsync(Client client, CancellationToken cancellationToken = default);
}

public interface IContractApiService
{
    Task<ApiResult<IReadOnlyList<Contract>>> SearchAsync(DateTime? startDate, DateTime? endDate, ContractStatus? status, CancellationToken cancellationToken = default);
    Task<ApiResult<Contract>> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResult<Contract>> CreateAsync(Contract contract, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> UpdateStatusAsync(int id, ContractStatus status, CancellationToken cancellationToken = default);
    Task<ApiResult<bool>> UploadAgreementAsync(int id, IFormFile file, CancellationToken cancellationToken = default);
    Task<ApiResult<DownloadedFile>> DownloadAgreementAsync(int id, CancellationToken cancellationToken = default);
}

public interface IServiceRequestApiService
{
    Task<ApiResult<IReadOnlyList<ServiceRequest>>> GetAsync(int? contractId = null, CancellationToken cancellationToken = default);
    Task<ApiResult<ServiceRequest>> CreateAsync(ServiceRequest request, CancellationToken cancellationToken = default);
}

public interface IExchangeRateApiService
{
    Task<ApiResult<ExchangeRateApiResponse>> GetUsdZarAsync(CancellationToken cancellationToken = default);
}

public interface IAuthApiService
{
    Task<ApiResult<LoginApiResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}

public abstract class ApiClientBase(HttpClient httpClient, ILogger logger)
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return ApiResult<T>.Failure(await ReadErrorAsync(response), (int)response.StatusCode);
            var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            return value is null ? ApiResult<T>.Failure("The API returned an empty response.", (int)response.StatusCode) : ApiResult<T>.Success(value, (int)response.StatusCode);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "The API request failed.");
            return ApiResult<T>.Failure("The service is temporarily unavailable. Please try again.");
        }
        catch (TaskCanceledException exception)
        {
            logger.LogWarning(exception, "The API request timed out or was cancelled.");
            return ApiResult<T>.Failure("The service is temporarily unavailable. Please try again.");
        }
    }

    protected static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
            return problem?.Detail ?? $"The API request failed with status {(int)response.StatusCode}.";
        }
        catch (JsonException)
        {
            return $"The API request failed with status {(int)response.StatusCode}.";
        }
    }

    private sealed record ProblemResponse(string? Detail);
}

public sealed class ClientApiService(HttpClient client, ILogger<ClientApiService> logger) : ApiClientBase(client, logger), IClientApiService
{
    public async Task<ApiResult<IReadOnlyList<Client>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<List<Client>>(new HttpRequestMessage(HttpMethod.Get, "api/clients"), cancellationToken);
        return result.IsSuccess ? ApiResult<IReadOnlyList<Client>>.Success(result.Value!, result.StatusCode) : ApiResult<IReadOnlyList<Client>>.Failure(result.ErrorMessage!, result.StatusCode);
    }

    public Task<ApiResult<Client>> CreateAsync(Client client, CancellationToken cancellationToken = default) =>
        SendAsync<Client>(new HttpRequestMessage(HttpMethod.Post, "api/clients") { Content = JsonContent.Create(new { client.Name, client.ContactDetails, client.Region }, options: JsonOptions) }, cancellationToken);
}

public sealed class ContractApiService(HttpClient client, ILogger<ContractApiService> logger) : ApiClientBase(client, logger), IContractApiService
{
    public async Task<ApiResult<IReadOnlyList<Contract>>> SearchAsync(DateTime? startDate, DateTime? endDate, ContractStatus? status, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (startDate.HasValue) query.Add($"startFrom={Uri.EscapeDataString(startDate.Value.ToString("O"))}");
        if (endDate.HasValue) query.Add($"endTo={Uri.EscapeDataString(endDate.Value.ToString("O"))}");
        if (status.HasValue) query.Add($"status={status}");
        var result = await SendAsync<List<Contract>>(new HttpRequestMessage(HttpMethod.Get, $"api/contracts?{string.Join("&", query)}"), cancellationToken);
        return result.IsSuccess ? ApiResult<IReadOnlyList<Contract>>.Success(result.Value!, result.StatusCode) : ApiResult<IReadOnlyList<Contract>>.Failure(result.ErrorMessage!, result.StatusCode);
    }

    public Task<ApiResult<Contract>> GetAsync(int id, CancellationToken cancellationToken = default) => SendAsync<Contract>(new HttpRequestMessage(HttpMethod.Get, $"api/contracts/{id}"), cancellationToken);

    public Task<ApiResult<Contract>> CreateAsync(Contract contract, CancellationToken cancellationToken = default) =>
        SendAsync<Contract>(new HttpRequestMessage(HttpMethod.Post, "api/contracts") { Content = JsonContent.Create(new { contract.ClientId, contract.StartDate, contract.EndDate, contract.Status, contract.ServiceLevel }, options: JsonOptions) }, cancellationToken);

    public async Task<ApiResult<bool>> UpdateStatusAsync(int id, ContractStatus status, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"api/contracts/{id}/status") { Content = JsonContent.Create(new { status }, options: JsonOptions) };
        var result = await SendAsync<Contract>(request, cancellationToken);
        return result.IsSuccess ? ApiResult<bool>.Success(true, result.StatusCode) : ApiResult<bool>.Failure(result.ErrorMessage!, result.StatusCode);
    }

    public async Task<ApiResult<bool>> UploadAgreementAsync(int id, IFormFile file, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        content.Add(new StreamContent(stream), "file", file.FileName);
        var result = await SendAsync<JsonElement>(new HttpRequestMessage(HttpMethod.Post, $"api/contracts/{id}/agreement") { Content = content }, cancellationToken);
        return result.IsSuccess ? ApiResult<bool>.Success(true, result.StatusCode) : ApiResult<bool>.Failure(result.ErrorMessage!, result.StatusCode);
    }

    public async Task<ApiResult<DownloadedFile>> DownloadAgreementAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetAsync($"api/contracts/{id}/agreement", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode) return ApiResult<DownloadedFile>.Failure(await ReadErrorAsync(response), (int)response.StatusCode);
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar?.Trim('"') ?? $"agreement-{id}.pdf";
            return ApiResult<DownloadedFile>.Success(new DownloadedFile(await response.Content.ReadAsStreamAsync(cancellationToken), fileName, response), (int)response.StatusCode);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Agreement download failed.");
            return ApiResult<DownloadedFile>.Failure("The service is temporarily unavailable. Please try again.");
        }
        catch (TaskCanceledException exception)
        {
            logger.LogWarning(exception, "Agreement download timed out.");
            return ApiResult<DownloadedFile>.Failure("The service is temporarily unavailable. Please try again.");
        }
    }
}

public sealed class ServiceRequestApiService(HttpClient client, ILogger<ServiceRequestApiService> logger) : ApiClientBase(client, logger), IServiceRequestApiService
{
    public async Task<ApiResult<IReadOnlyList<ServiceRequest>>> GetAsync(int? contractId = null, CancellationToken cancellationToken = default)
    {
        var path = contractId.HasValue ? $"api/servicerequests?contractId={contractId}" : "api/servicerequests";
        var result = await SendAsync<List<ServiceRequest>>(new HttpRequestMessage(HttpMethod.Get, path), cancellationToken);
        return result.IsSuccess ? ApiResult<IReadOnlyList<ServiceRequest>>.Success(result.Value!, result.StatusCode) : ApiResult<IReadOnlyList<ServiceRequest>>.Failure(result.ErrorMessage!, result.StatusCode);
    }

    public Task<ApiResult<ServiceRequest>> CreateAsync(ServiceRequest request, CancellationToken cancellationToken = default) =>
        SendAsync<ServiceRequest>(new HttpRequestMessage(HttpMethod.Post, "api/servicerequests") { Content = JsonContent.Create(new { request.ContractId, request.Description, AmountUsd = request.UsdAmount, request.Status }, options: JsonOptions) }, cancellationToken);
}

public sealed class ExchangeRateApiService(HttpClient client, ILogger<ExchangeRateApiService> logger) : ApiClientBase(client, logger), IExchangeRateApiService
{
    public Task<ApiResult<ExchangeRateApiResponse>> GetUsdZarAsync(CancellationToken cancellationToken = default) => SendAsync<ExchangeRateApiResponse>(new HttpRequestMessage(HttpMethod.Get, "api/exchangerates/usd-zar"), cancellationToken);
}

public sealed class AuthApiService(HttpClient client, ILogger<AuthApiService> logger) : ApiClientBase(client, logger), IAuthApiService
{
    public Task<ApiResult<LoginApiResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default) =>
        SendAsync<LoginApiResponse>(new HttpRequestMessage(HttpMethod.Post, "api/auth/login") { Content = JsonContent.Create(new { email, password }) }, cancellationToken);
}

public sealed class JwtBearerHandler(IHttpContextAccessor accessor, ILogger<JwtBearerHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = accessor.HttpContext is null
            ? null
            : await accessor.HttpContext.GetTokenAsync("access_token");
        if (!string.IsNullOrWhiteSpace(token)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning("The API rejected the current authentication token.");
            accessor.HttpContext?.Response.Cookies.Delete(".AspNetCore.Cookies");
        }
        return response;
    }
}
