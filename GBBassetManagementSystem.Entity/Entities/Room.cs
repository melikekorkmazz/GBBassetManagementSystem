using System.ComponentModel.DataAnnotations;
using GBBassetManagementSystem.Core.Entities;

namespace GBBassetManagementSystem.Entity.Entities;

public class Room : EntityBase
{
    [Required(ErrorMessage = "RoomNameRequired")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "RoomNumberRequired")]
    public string RoomNumber { get; set; } = string.Empty;

    public string? Floor { get; set; }

    public string? Building { get; set; }

    public Guid DepartmentId { get; set; }

    public Department? Department { get; set; }
}