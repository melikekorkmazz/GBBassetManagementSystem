using GBBassetManagementSystem.Core.Entities;

namespace GBBassetManagementSystem.Entity.Entities;

public class Department : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}