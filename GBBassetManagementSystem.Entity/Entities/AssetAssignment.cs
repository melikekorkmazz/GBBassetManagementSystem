using GBBassetManagementSystem.Core.Entities;
using GBBassetManagementSystem.Entity.Enums;
using System.ComponentModel.DataAnnotations;
namespace GBBassetManagementSystem.Entity.Entities;

public class AssetAssignment : EntityBase
{
     public Guid? AssetId { get; set; }

    public Asset? Asset { get; set; }

    public AssignmentType AssignmentType { get; set; }

    public Guid? PersonnelId { get; set; }

    public Personnel? Personnel { get; set; }

    public Guid? RoomId { get; set; }

    public Room? Room { get; set; }

[Required(ErrorMessage = "DeliveredByRequired")]
public string DeliveredBy { get; set; } = string.Empty;
    public string? ReceivedBy { get; set; }
    public DateTime AssignmentDate { get; set; } = DateTime.Today;

    public DateTime? ExpectedReturnDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}