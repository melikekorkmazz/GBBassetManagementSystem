using GBBassetManagementSystem.Core.Entities;

namespace GBBassetManagementSystem.Entity.Entities;

public class Personnel : EntityBase
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }

    public Department? Department { get; set; }
}