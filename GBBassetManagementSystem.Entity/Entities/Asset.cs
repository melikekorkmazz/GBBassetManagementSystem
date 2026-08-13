using System.ComponentModel.DataAnnotations;
using GBBassetManagementSystem.Core.Entities;
using GBBassetManagementSystem.Entity.Enums;

namespace GBBassetManagementSystem.Entity.Entities;

public class Asset : EntityBase
{
    [Display(Name = "AssetCode")]
    public string AssetCode { get; set; } = string.Empty;

    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Brand")]
    public string Brand { get; set; } = string.Empty;

    [Display(Name = "Model")]
    public string Model { get; set; } = string.Empty;

    [Display(Name = "SerialNumber")]
    public string SerialNumber { get; set; } = string.Empty;

    [Display(Name = "PurchaseDate")]
    public DateTime PurchaseDate { get; set; }

    [Display(Name = "PurchasePrice")]
    public decimal PurchasePrice { get; set; }

    [Display(Name = "Category")]
    public Guid CategoryId { get; set; }

    public Category? Category { get; set; }

    [Display(Name = "Status")]
    public AssetStatus Status { get; set; } = AssetStatus.Available;

    [Display(Name = "WarrantyExpiration")]
    public DateTime? WarrantyExpirationDate { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }
}