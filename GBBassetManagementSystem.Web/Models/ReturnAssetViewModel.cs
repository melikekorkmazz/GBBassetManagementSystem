using System.ComponentModel.DataAnnotations;

namespace GBBassetManagementSystem.Web.Models;

public class ReturnAssetViewModel
{
    public Guid AssignmentId { get; set; }

    public string AssetDisplayName { get; set; } = string.Empty;

    public string AssignedTo { get; set; } = string.Empty;

    [Required]
    public DateTime ReturnDate { get; set; } = DateTime.Today;

    [Required]
    public string ReceivedBy { get; set; } = string.Empty;

    [Required]
    public string Condition { get; set; } = "Good";

    public string? DamageDescription { get; set; }

    public string? Notes { get; set; }
}