namespace TechMove.Api.Services;

public class BusinessRuleException(string message) : Exception(message);
public class NotFoundException(string message) : Exception(message);
