using GBBassetManagementSystem.Entity.Enums;
using GBBassetManagementSystem.Core.Entities;
namespace GBBassetManagementSystem.Entity.Entities;

public class Asset : EntityBase
{
    public string AssetCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public DateTime PurchaseDate { get; set; }

    public decimal PurchasePrice { get; set; }

    public Guid CategoryId { get; set; }

    public  Category? Category { get; set; } 

    public AssetStatus Status { get; set; } = AssetStatus.Available;

    public DateTime? WarrantyExpirationDate { get; set; }

    public string? Notes { get; set; }
    
}