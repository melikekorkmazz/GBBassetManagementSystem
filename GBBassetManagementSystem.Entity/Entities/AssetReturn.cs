using GBBassetManagementSystem.Core.Entities;

namespace GBBassetManagementSystem.Entity.Entities;

public class AssetReturn : EntityBase
{
    public Guid AssetAssignmentId { get; set; }

    public AssetAssignment AssetAssignment { get; set; } = null!;

    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

     public string? ReceivedBy { get; set; }
    public string Condition { get; set; } = string.Empty;

    public string? DamageDescription { get; set; }

    public string? Notes { get; set; }
}