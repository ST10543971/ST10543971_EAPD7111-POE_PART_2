using TechMove.Api.Models;
using TechMove.Api.Repositories;

namespace TechMove.Api.Services;

public sealed class ServiceRequestService(IServiceRequestRepository repository, IContractRepository contractRepository, ICurrencyService currencyService) : IServiceRequestService
{
    public Task<IReadOnlyList<ServiceRequest>> GetAsync(int? contractId, CancellationToken cancellationToken = default) => repository.GetAsync(contractId, cancellationToken);

    public async Task<ServiceRequest> GetAsync(int id, CancellationToken cancellationToken = default) =>
        await repository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException($"Service request {id} was not found.");

    public async Task<ServiceRequest> CreateAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        var contract = await contractRepository.GetByIdAsync(serviceRequest.ContractId, cancellationToken)
            ?? throw new NotFoundException($"Contract {serviceRequest.ContractId} was not found.");
        if (contract.Status is ContractStatus.Expired or ContractStatus.OnHold)
            throw new BusinessRuleException("Cannot create a service request for an expired or on-hold contract.");
        if (serviceRequest.UsdAmount < 0) throw new BusinessRuleException("The USD amount cannot be negative.");

        serviceRequest.ExchangeRate = await currencyService.GetUsdToZarRateAsync(cancellationToken);
        serviceRequest.Cost = decimal.Round(serviceRequest.UsdAmount * serviceRequest.ExchangeRate, 2);
        return await repository.AddAsync(serviceRequest, cancellationToken);
    }
}
