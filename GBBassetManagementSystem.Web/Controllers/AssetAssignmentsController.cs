using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Entity.Enums;
using GBBassetManagementSystem.Service.Interfaces;
using GBBassetManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GBBassetManagementSystem.Web.Controllers;

public class AssetAssignmentsController : Controller
{
    private readonly IAssetAssignmentService _assignmentService;
    private readonly IAssetService _assetService;
    private readonly IPersonnelService _personnelService;
    private readonly IRoomService _roomService;

    public AssetAssignmentsController(
        IAssetAssignmentService assignmentService,
        IAssetService assetService,
        IPersonnelService personnelService,
        IRoomService roomService)
    {
        _assignmentService = assignmentService;
        _assetService = assetService;
        _personnelService = personnelService;
        _roomService = roomService;
    }

    public async Task<IActionResult> Index()
    {
        var assignments = await _assignmentService.GetAllAsync();

        return View(assignments);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var assignment =
            await _assignmentService.GetByIdAsync(id);

        if (assignment is null)
        {
            return NotFound();
        }

        return View(assignment);
    }

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

    public async Task<IActionResult> Return(Guid id)
    {
        var assignment =
            await _assignmentService.GetByIdAsync(id);

        if (assignment is null)
        {
            return NotFound();
        }

        if (!assignment.IsActive)
        {
            TempData["SuccessMessage"] =
                "This asset has already been returned.";

            return RedirectToAction(nameof(Index));
        }

        var assignedTo =
            assignment.AssignmentType == AssignmentType.Personnel
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
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _assignmentService.ReturnAsync(
                model.AssignmentId,
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

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Asset returned successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadFormDataAsync(
        Guid? selectedAssetId = null,
        Guid? selectedPersonnelId = null,
        Guid? selectedRoomId = null)
    {
        var assets = await _assetService.GetAllAsync();

        var availableAssets = assets
            .Where(a => a.Status == AssetStatus.Available)
            .Select(a => new
            {
                a.Id,
                DisplayName = $"{a.AssetCode} - {a.Name}"
            })
            .ToList();

        ViewBag.Assets = new SelectList(
            availableAssets,
            "Id",
            "DisplayName",
            selectedAssetId);

        var personnel =
            await _personnelService.GetAllAsync();

        var personnelOptions = personnel
            .Select(p => new
            {
                p.Id,
                FullName = $"{p.FirstName} {p.LastName}"
            })
            .ToList();

        ViewBag.Personnel = new SelectList(
            personnelOptions,
            "Id",
            "FullName",
            selectedPersonnelId);

        var rooms = await _roomService.GetAllAsync();

        var roomOptions = rooms
            .Select(r => new
            {
                r.Id,
                DisplayName =
                    $"{r.Building} - {r.RoomNumber} - {r.Name}"
            })
            .ToList();

        ViewBag.Rooms = new SelectList(
            roomOptions,
            "Id",
            "DisplayName",
            selectedRoomId);
    }
}