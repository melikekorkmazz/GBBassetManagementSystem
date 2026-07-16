using GBBassetManagementSystem.Entity.Entities;

namespace GBBassetManagementSystem.Web.Models;

public class PersonnelDetailsViewModel
{
    public Personnel Personnel { get; set; } = null!;

    public List<AssetAssignment> AssignmentHistory { get; set; } = [];

    public int TotalAssignments =>
        AssignmentHistory.Count;

    public int ActiveAssignments =>
        AssignmentHistory.Count(assignment => assignment.IsActive);

    public int ComputerAssignments =>
        AssignmentHistory.Count(assignment =>
            assignment.Asset?.Category?.Name.Contains(
                "Computer",
                StringComparison.OrdinalIgnoreCase) == true ||
            assignment.Asset?.Category?.Name.Contains(
                "Laptop",
                StringComparison.OrdinalIgnoreCase) == true);

    public int FurnitureAssignments =>
        AssignmentHistory.Count(assignment =>
            assignment.Asset?.Category?.Name.Contains(
                "Furniture",
                StringComparison.OrdinalIgnoreCase) == true);
}