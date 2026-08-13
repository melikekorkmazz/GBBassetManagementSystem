using Microsoft.AspNetCore.Identity;

namespace GBBassetManagementSystem.Entity.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // The department assigned to this user.
    // Admin users may have no department.
    public Guid? DepartmentId { get; set; }

    // Navigation property for the user's department.
    public Department? Department { get; set; }

    // Determines whether the account is archived.
    public bool IsArchived { get; set; } = false;

    // Date when the account was archived.
    public DateTime? ArchivedDate { get; set; }
}