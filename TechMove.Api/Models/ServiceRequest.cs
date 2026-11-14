namespace TechMove.Api.Models;

public class ServiceRequest
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public Contract? Contract { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UsdAmount { get; set; }
    public decimal Cost { get; set; }
    public decimal ExchangeRate { get; set; }
    public string Status { get; set; } = "Pending";
}
