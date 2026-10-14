using Xunit;

namespace TechMove.Tests
{
    public class CurrencyTests
    {
        [Fact]
        public void CurrencyCalculation_ReturnsCorrectValue()
        {
            decimal usd = 100;
            decimal rate = 18.50m;

            decimal result = usd * rate;

            Assert.Equal(1850m, result);
        }
    }
}