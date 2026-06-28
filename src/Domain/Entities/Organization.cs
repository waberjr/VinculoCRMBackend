namespace VinculoBackend.Domain.Entities;

public class Organization : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Document { get; set; }
    public decimal? DefaultMonthlyGoal { get; set; }
    public string TimeZone { get; set; } = "America/Sao_Paulo";
    public string Currency { get; set; } = "BRL";
    public bool IsActive { get; set; } = true;
}
