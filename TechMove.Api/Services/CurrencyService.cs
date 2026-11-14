using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;

namespace TechMove.Api.Services;

public sealed class CurrencyService(HttpClient httpClient, IMemoryCache cache, ILogger<CurrencyService> logger) : ICurrencyService
{
    private const string CacheKey = "usd-zar-rate";

    public async Task<decimal> GetUsdToZarRateAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out decimal cachedRate))
        {
            return cachedRate;
        }

        try
        {
            var response = await httpClient.GetFromJsonAsync<ExchangeRateResponse>(
                "v6/latest/USD", cancellationToken);
            var rate = response?.Rates?.GetValueOrDefault("ZAR") ?? 0m;
            if (rate <= 0)
            {
                throw new InvalidOperationException("The exchange-rate response did not contain a valid ZAR rate.");
            }

            cache.Set(CacheKey, rate, TimeSpan.FromMinutes(10));
            return rate;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogError(exception, "Unable to retrieve the USD to ZAR exchange rate.");
            throw;
        }
    }

    private sealed class ExchangeRateResponse
    {
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}
