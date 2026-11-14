using TechMove.Api.Models;

namespace TechMove.Api.Dtos;

public static class Mappings
{
    public static ClientDto ToDto(this Client client) => new(client.Id, client.Name, client.ContactDetails, client.Region);

    public static ContractDto ToDto(this Contract contract) => new(contract.Id, contract.ClientId, contract.Client?.Name, contract.StartDate, contract.EndDate, contract.Status, contract.ServiceLevel, contract.SignedAgreementPath);

    public static ServiceRequestDto ToDto(this ServiceRequest request) => new(request.Id, request.ContractId, request.Description, request.UsdAmount, request.Cost, request.ExchangeRate, request.Status);
}
