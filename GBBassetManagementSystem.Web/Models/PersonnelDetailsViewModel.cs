using GBBassetManagementSystem.Entity.Entities;

namespace GBBassetManagementSystem.Web.Models;

public class PersonnelDetailsViewModel
{
    public Personnel Personnel { get; set; } = null!;

    public List<AssetAssignment> AssignmentHistory { get; set; } = [];

    // Total assignments made to this personnel
    public int TotalAssignments =>
        AssignmentHistory.Count;

    // Currently assigned assets
    public int ActiveAssignments =>
        AssignmentHistory.Count(x => x.IsActive);

    // Returned assets
    public int ReturnedAssignments =>
        AssignmentHistory.Count(x => !x.IsActive);

    // Last assignment date
    public DateTime? LastAssignmentDate =>
        AssignmentHistory
            .OrderByDescending(x => x.AssignmentDate)
            .Select(x => (DateTime?)x.AssignmentDate)
            .FirstOrDefault();

    // Computer & Laptop assignments
    public int ComputerAssignments =>
        AssignmentHistory.Count(x =>
            x.Asset?.Category?.Name.Contains("Computer", StringComparison.OrdinalIgnoreCase) == true ||
            x.Asset?.Category?.Name.Contains("Laptop", StringComparison.OrdinalIgnoreCase) == true);

    // Furniture assignments
    public int FurnitureAssignments =>
        AssignmentHistory.Count(x =>
            x.Asset?.Category?.Name.Contains("Furniture", StringComparison.OrdinalIgnoreCase) == true);

    // Printer assignments
    public int PrinterAssignments =>
        AssignmentHistory.Count(x =>
            x.Asset?.Category?.Name.Contains("Printer", StringComparison.OrdinalIgnoreCase) == true);

    // Monitor assignments
    public int MonitorAssignments =>
        AssignmentHistory.Count(x =>
            x.Asset?.Category?.Name.Contains("Monitor", StringComparison.OrdinalIgnoreCase) == true);
}