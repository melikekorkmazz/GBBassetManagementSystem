using GBBassetManagementSystem.Core.Entities;

namespace GBBassetManagementSystem.Entity.Entities;

public class Room : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public string RoomNumber { get; set; } = string.Empty;

    public string? Floor { get; set; }

    public string? Building { get; set; }

    public Guid DepartmentId { get; set; }

    public Department? Department { get; set; }
}