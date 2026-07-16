using GBBassetManagementSystem.Entity.Entities;

namespace GBBassetManagementSystem.Web.Models;

public class AssetDetailsViewModel
{
    public Asset Asset { get; set; } = null!;

    public List<AssetAssignment> AssignmentHistory { get; set; } = [];
}