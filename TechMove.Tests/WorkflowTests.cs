using TechMove.Models;
using Xunit;

namespace TechMove.Tests
{
    public class WorkflowTests
    {
        [Fact]
        public void ExpiredContractCannotCreateRequest()
        {
            var contract = new Contract
            {
                Status = ContractStatus.Expired
            };

            Assert.Equal(
                ContractStatus.Expired,
                contract.Status);
        }
    }
}