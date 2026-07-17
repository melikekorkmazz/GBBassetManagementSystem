namespace GBBassetManagementSystem.Web.Models;

public class InventoryViewModel
{
    public int TotalAssets { get; set; }

    public int AvailableAssets { get; set; }

    public int AssignedAssets { get; set; }

    public int BrokenAssets { get; set; }

    public int UnderMaintenanceAssets { get; set; }

    public int LostAssets { get; set; }

    public List<InventoryCategorySummaryViewModel> Categories { get; set; } = [];
}

public class InventoryCategorySummaryViewModel
{
    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int Total { get; set; }

    public int Available { get; set; }

    public int Assigned { get; set; }

    public int Broken { get; set; }

    public int UnderMaintenance { get; set; }

    public int Lost { get; set; }
}