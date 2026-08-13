using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Entity.Enums;
using GBBassetManagementSystem.Service.Interfaces;
using GBBassetManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using GBBassetManagementSystem.Web;

namespace GBBassetManagementSystem.Web.Controllers;

[Authorize(Roles = "Admin,DepartmentUser")]
public class AssetAssignmentsController : Controller
{
    private readonly IAssetAssignmentService _assignmentService;
    private readonly IAssetService _assetService;
    private readonly IPersonnelService _personnelService;
    private readonly IRoomService _roomService;
    private readonly IDepartmentService _departmentService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<SharedResource> _localizer;
    public AssetAssignmentsController(
        IAssetAssignmentService assignmentService,
        IAssetService assetService,
        IPersonnelService personnelService,
        IRoomService roomService,
        IDepartmentService departmentService,
        UserManager<ApplicationUser> userManager,
        IStringLocalizer<SharedResource> localizer)
    {
        _assignmentService = assignmentService;
        _assetService = assetService;
        _personnelService = personnelService;
        _roomService = roomService;
        _departmentService = departmentService;
        _userManager = userManager;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var assignments =
            await _assignmentService.GetAllAsync();

        if (User.IsInRole("DepartmentUser"))
        {
            var departmentId =
                await GetCurrentDepartmentIdAsync();

            if (!departmentId.HasValue)
            {
                return Forbid();
            }

            assignments = assignments
                .Where(assignment =>
                    GetAssignmentDepartmentId(assignment) ==
                    departmentId.Value)
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchValue = search.Trim();

            assignments = assignments
                .Where(assignment =>
                    ContainsText(
                        assignment.Asset?.AssetCode,
                        searchValue) ||
                    ContainsText(
                        assignment.Asset?.Name,
                        searchValue) ||
                    ContainsText(
                        assignment.Personnel?.FirstName,
                        searchValue) ||
                    ContainsText(
                        assignment.Personnel?.LastName,
                        searchValue) ||
                    ContainsText(
                        assignment.Personnel?.Department?.Name,
                        searchValue) ||
                    ContainsText(
                        assignment.Room?.Name,
                        searchValue) ||
                    ContainsText(
                        assignment.AssignmentType.ToString(),
                        searchValue))
                .ToList();
        }

        ViewBag.Search = search;

        return View(assignments);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var assignment =
            await _assignmentService.GetByIdAsync(id);

        if (assignment is null)
        {
            return NotFound();
        }

        if (!await CanAccessAssignmentAsync(assignment))
        {
            return Forbid();
        }

        return View(assignment);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadFormDataAsync();

        return View(new AssetAssignment
        {
            AssignmentDate = DateTime.Today,
            AssignmentType = AssignmentType.Personnel
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AssetAssignment assignment)
    {
        if (!await CanUseAssignmentTargetAsync(assignment))
        {
            return Forbid();
        }
if (!assignment.AssetId.HasValue)
{
    ModelState.AddModelError(
        nameof(assignment.AssetId),
        _localizer["AssetRequired"].Value);
}

if (string.IsNullOrWhiteSpace(assignment.DeliveredBy))
{
    ModelState.AddModelError(
        nameof(assignment.DeliveredBy),
        _localizer["DeliveredByRequired"].Value);
}
        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(
                assignment.AssetId,
                assignment.PersonnelId,
                assignment.RoomId);

            return View(assignment);
        }

        try
        {
            await _assignmentService.AssignAsync(assignment);
        }
        catch (Exception exception)
            when (exception is KeyNotFoundException
                  or InvalidOperationException)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            await LoadFormDataAsync(
                assignment.AssetId,
                assignment.PersonnelId,
                assignment.RoomId);

            return View(assignment);
        }

        TempData["SuccessMessage"] =
            "Asset assigned successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetPersonnelByDepartment(
        Guid departmentId)
    {
        if (!await CanAccessDepartmentAsync(departmentId))
        {
            return Forbid();
        }

        var personnel =
            await _personnelService.GetAllAsync();

        var personnelOptions = personnel
            .Where(person =>
                person.DepartmentId == departmentId)
            .OrderBy(person => person.FirstName)
            .ThenBy(person => person.LastName)
            .Select(person => new
            {
                id = person.Id,
                fullName =
                    $"{person.FirstName} {person.LastName}"
            })
            .ToList();

        return Json(personnelOptions);
    }

    [HttpGet]
    public async Task<IActionResult> GetRoomsByDepartment(
        Guid departmentId)
    {
        if (!await CanAccessDepartmentAsync(departmentId))
        {
            return Forbid();
        }

        var rooms =
            await _roomService.GetAllAsync();

        var roomOptions = rooms
            .Where(room =>
                room.DepartmentId == departmentId)
            .OrderBy(room => room.Building)
            .ThenBy(room => room.RoomNumber)
            .Select(room => new
            {
                id = room.Id,
                name =
                    $"{room.Building} - " +
                    $"{room.RoomNumber} - " +
                    $"{room.Name}"
            })
            .ToList();

        return Json(roomOptions);
    }

    [HttpGet]
    public async Task<IActionResult> Return(Guid id)
    {
        var assignment =
            await _assignmentService.GetByIdAsync(id);

        if (assignment is null)
        {
            return NotFound();
        }

        if (!await CanAccessAssignmentAsync(assignment))
        {
            return Forbid();
        }

        if (!assignment.IsActive)
        {
           TempData["SuccessMessage"] =
    _localizer["AssetReturnedSuccessfully"].Value;
            return RedirectToAction(nameof(Index));
        }

        var assignedTo =
            assignment.AssignmentType ==
            AssignmentType.Personnel
                ? $"{assignment.Personnel?.FirstName} " +
                  $"{assignment.Personnel?.LastName}"
                : $"{assignment.Room?.Name} " +
                  $"({assignment.Room?.RoomNumber})";

        var model = new ReturnAssetViewModel
        {
            AssignmentId = assignment.Id,

            AssetDisplayName =
                $"{assignment.Asset?.AssetCode} - " +
                $"{assignment.Asset?.Name}",

            AssignedTo = assignedTo,

            ReturnDate = DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(
        ReturnAssetViewModel model)
    {
        var assignment =
            await _assignmentService.GetByIdAsync(
                model.AssignmentId);

        if (assignment is null)
        {
            return NotFound();
        }

        if (!await CanAccessAssignmentAsync(assignment))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            PopulateReturnDisplayData(model, assignment);

            return View(model);
        }

        try
        {
            await _assignmentService.ReturnAsync(
    model.AssignmentId,
    model.ReturnDate,
    model.ReceivedBy,
    model.Condition,
    model.DamageDescription,
    model.Notes);
        }
        catch (Exception exception)
            when (exception is KeyNotFoundException
                  or InvalidOperationException)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            PopulateReturnDisplayData(model, assignment);

            return View(model);
        }

        TempData["SuccessMessage"] =
            _localizer["AssetReturnedSuccessfully"].Value;

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadFormDataAsync(
        Guid? selectedAssetId = null,
        Guid? selectedPersonnelId = null,
        Guid? selectedRoomId = null)
    {
        await LoadAssetsAsync(selectedAssetId);

        var departments =
            await _departmentService.GetAllAsync();

        var personnel =
            await _personnelService.GetAllAsync();

        var rooms =
            await _roomService.GetAllAsync();

        if (User.IsInRole("DepartmentUser"))
        {
            var departmentId =
                await GetCurrentDepartmentIdAsync();

            departments = departmentId.HasValue
                ? departments
                    .Where(department =>
                        department.Id ==
                        departmentId.Value)
                    .ToList()
                : [];

            personnel = departmentId.HasValue
                ? personnel
                    .Where(person =>
                        person.DepartmentId ==
                        departmentId.Value)
                    .ToList()
                : [];

            rooms = departmentId.HasValue
                ? rooms
                    .Where(room =>
                        room.DepartmentId ==
                        departmentId.Value)
                    .ToList()
                : [];
        }

        ViewBag.Departments = new SelectList(
            departments.OrderBy(department =>
                department.Name),
            "Id",
            "Name");

        var personnelOptions = personnel
            .OrderBy(person => person.FirstName)
            .ThenBy(person => person.LastName)
            .Select(person => new
            {
                person.Id,

                FullName =
                    $"{person.FirstName} " +
                    $"{person.LastName}"
            })
            .ToList();

        ViewBag.Personnel = new SelectList(
            personnelOptions,
            "Id",
            "FullName",
            selectedPersonnelId);

        var roomOptions = rooms
            .OrderBy(room => room.Building)
            .ThenBy(room => room.RoomNumber)
            .Select(room => new
            {
                room.Id,

                DisplayName =
                    $"{room.Building} - " +
                    $"{room.RoomNumber} - " +
                    $"{room.Name}"
            })
            .ToList();

        ViewBag.Rooms = new SelectList(
            roomOptions,
            "Id",
            "DisplayName",
            selectedRoomId);
    }

    private async Task LoadAssetsAsync(
        Guid? selectedAssetId)
    {
        var assets =
            await _assetService.GetAllAsync();

        var availableAssets = assets
            .Where(asset =>
                asset.Status == AssetStatus.Available ||
                asset.Id == selectedAssetId)
            .OrderBy(asset => asset.AssetCode)
            .Select(asset => new
            {
                asset.Id,

                DisplayName =
                    $"{asset.AssetCode} - {asset.Name}"
            })
            .ToList();

        ViewBag.Assets = new SelectList(
            availableAssets,
            "Id",
            "DisplayName",
            selectedAssetId);
    }

    private async Task<bool> CanUseAssignmentTargetAsync(
        AssetAssignment assignment)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var currentDepartmentId =
            await GetCurrentDepartmentIdAsync();

        if (!currentDepartmentId.HasValue)
        {
            return false;
        }

        if (assignment.AssignmentType ==
            AssignmentType.Personnel)
        {
            if (!assignment.PersonnelId.HasValue)
            {
                return true;
            }

            var personnel =
                await _personnelService.GetByIdAsync(
                    assignment.PersonnelId.Value);

            return personnel is not null &&
                   personnel.DepartmentId ==
                   currentDepartmentId.Value;
        }

        if (assignment.AssignmentType ==
            AssignmentType.Room)
        {
            if (!assignment.RoomId.HasValue)
            {
                return true;
            }

            var room =
                await _roomService.GetByIdAsync(
                    assignment.RoomId.Value);

            return room is not null &&
                   room.DepartmentId ==
                   currentDepartmentId.Value;
        }

        return false;
    }

    private async Task<bool> CanAccessAssignmentAsync(
        AssetAssignment assignment)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var currentDepartmentId =
            await GetCurrentDepartmentIdAsync();

        if (!currentDepartmentId.HasValue)
        {
            return false;
        }

        return GetAssignmentDepartmentId(assignment) ==
               currentDepartmentId.Value;
    }

    private async Task<bool> CanAccessDepartmentAsync(
        Guid departmentId)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var currentDepartmentId =
            await GetCurrentDepartmentIdAsync();

        return currentDepartmentId.HasValue &&
               currentDepartmentId.Value == departmentId;
    }

    private async Task<Guid?> GetCurrentDepartmentIdAsync()
    {
        var currentUser =
            await _userManager.GetUserAsync(User);

        return currentUser?.DepartmentId;
    }

    private static Guid? GetAssignmentDepartmentId(
        AssetAssignment assignment)
    {
        return assignment.AssignmentType switch
        {
            AssignmentType.Personnel =>
                assignment.Personnel?.DepartmentId,

            AssignmentType.Room =>
                assignment.Room?.DepartmentId,

            _ => null
        };
    }

    private static void PopulateReturnDisplayData(
        ReturnAssetViewModel model,
        AssetAssignment assignment)
    {
        model.AssetDisplayName =
            $"{assignment.Asset?.AssetCode} - " +
            $"{assignment.Asset?.Name}";

        model.AssignedTo =
            assignment.AssignmentType ==
            AssignmentType.Personnel
                ? $"{assignment.Personnel?.FirstName} " +
                  $"{assignment.Personnel?.LastName}"
                : $"{assignment.Room?.Name} " +
                  $"({assignment.Room?.RoomNumber})";
    }

    private static bool ContainsText(
        string? value,
        string search)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Contains(
                   search,
                   StringComparison.OrdinalIgnoreCase);
    }
}