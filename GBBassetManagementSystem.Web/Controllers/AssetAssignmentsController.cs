using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Entity.Enums;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GBBassetManagementSystem.Web.Controllers;

public class AssetAssignmentsController : Controller
{
    private readonly IAssetAssignmentService _assignmentService;
    private readonly IAssetService _assetService;
    private readonly IPersonnelService _personnelService;

    public AssetAssignmentsController(
        IAssetAssignmentService assignmentService,
        IAssetService assetService,
        IPersonnelService personnelService)
    {
        _assignmentService = assignmentService;
        _assetService = assetService;
        _personnelService = personnelService;
    }

    public async Task<IActionResult> Index()
    {
        var assignments = await _assignmentService.GetAllAsync();

        return View(assignments);
    }

    public async Task<IActionResult> Create()
    {
        await LoadFormDataAsync();

        return View(new AssetAssignment
        {
            AssignmentDate = DateTime.Today
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
                assignment.PersonnelId);

            return View(assignment);
        }

        try
        {
            await _assignmentService.AssignAsync(assignment);
        }
        catch (KeyNotFoundException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadFormDataAsync(
                assignment.AssetId,
                assignment.PersonnelId);

            return View(assignment);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadFormDataAsync(
                assignment.AssetId,
                assignment.PersonnelId);

            return View(assignment);
        }

        TempData["SuccessMessage"] =
            "Asset assigned successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadFormDataAsync(
        Guid? selectedAssetId = null,
        Guid? selectedPersonnelId = null)
    {
        var assets = await _assetService.GetAllAsync();

        var availableAssets = assets
            .Where(asset => asset.Status == AssetStatus.Available)
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

        var personnel = await _personnelService.GetAllAsync();

        var personnelOptions = personnel
            .Select(person => new
            {
                person.Id,
                FullName =
                    $"{person.FirstName} {person.LastName}"
            })
            .ToList();

        ViewBag.Personnel = new SelectList(
            personnelOptions,
            "Id",
            "FullName",
            selectedPersonnelId);
    }
}