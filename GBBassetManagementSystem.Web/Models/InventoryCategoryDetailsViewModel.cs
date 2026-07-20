using GBBassetManagementSystem.Entity.Enums;

namespace GBBassetManagementSystem.Web.Models;

public class InventoryCategoryDetailsViewModel
{
    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int Total { get; set; }

    public int Available { get; set; }

    public int Assigned { get; set; }

    public int Broken { get; set; }

    public int UnderMaintenance { get; set; }

    public int Lost { get; set; }

    public string? Search { get; set; }

    public AssetStatus? SelectedStatus { get; set; }

    public Guid? SelectedDepartmentId { get; set; }

    public Guid? SelectedRoomId { get; set; }

    public List<InventoryAssetRowViewModel> Assets { get; set; } = [];

    public List<InventoryDepartmentSummaryViewModel> Departments { get; set; } = [];

    public List<InventoryRoomSummaryViewModel> Rooms { get; set; } = [];
}

public class InventoryAssetRowViewModel
{
    public Guid AssetId { get; set; }

    public string AssetCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public AssetStatus Status { get; set; }

    public string AssignedTo { get; set; } = "—";

    public string DepartmentName { get; set; } = "—";

    public Guid? DepartmentId { get; set; }

    public string RoomName { get; set; } = "—";

    public Guid? RoomId { get; set; }

    public string Location { get; set; } = "—";
}

public class InventoryDepartmentSummaryViewModel
{
    public Guid DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public int AssetCount { get; set; }
}

public class InventoryRoomSummaryViewModel
{
    public Guid RoomId { get; set; }

    public string RoomName { get; set; } = string.Empty;

    public string RoomNumber { get; set; } = string.Empty;

    public string Building { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }

    public int AssetCount { get; set; }
}