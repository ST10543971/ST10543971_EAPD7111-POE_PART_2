using Newtonsoft.Json;

namespace TechMove.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetUsdToZarRate()
        {
            var response = await _httpClient.GetStringAsync(
                "https://open.er-api.com/v6/latest/USD");

            dynamic result =
                JsonConvert.DeserializeObject(response);

            return (decimal)result.rates.ZAR;
        }
    }
}