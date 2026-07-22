using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GBBassetManagementSystem.Web.Models;
namespace GBBassetManagementSystem.Web.Controllers;

public class PersonnelController : Controller
{
    private readonly IPersonnelService _personnelService;
    private readonly IDepartmentService _departmentService;
    private readonly IAssetAssignmentService _assignmentService;

    public PersonnelController(
    IPersonnelService personnelService,
    IDepartmentService departmentService,
    IAssetAssignmentService assignmentService)
{
    _personnelService = personnelService;
    _departmentService = departmentService;
    _assignmentService = assignmentService;
}

    public async Task<IActionResult> Index()
    {
        var personnel = await _personnelService.GetAllAsync();

        return View(personnel);
    }

    public async Task<IActionResult> Create()
    {
        await LoadDepartmentsAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Personnel personnel)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(personnel.DepartmentId);

            return View(personnel);
        }

        await _personnelService.AddAsync(personnel);

        TempData["SuccessMessage"] = "Personnel saved successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var personnel = await _personnelService.GetByIdAsync(id);

        if (personnel is null)
        {
            return NotFound();
        }

        await LoadDepartmentsAsync(personnel.DepartmentId);

        return View(personnel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Personnel personnel)
    {
        if (id != personnel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(personnel.DepartmentId);

            return View(personnel);
        }

        try
        {
            await _personnelService.UpdateAsync(personnel);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Personnel updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var personnel = await _personnelService.GetByIdAsync(id);

        if (personnel is null)
        {
            return NotFound();
        }

        return View(personnel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            await _personnelService.DeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Personnel Deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDepartmentsAsync(
        Guid? selectedDepartmentId = null)
    {
        var departments = await _departmentService.GetAllAsync();

        ViewBag.Departments = new SelectList(
            departments,
            "Id",
            "Name",
            selectedDepartmentId);
    }
    public async Task<IActionResult> Details(Guid id)
{
    var personnel = await _personnelService.GetByIdAsync(id);

    if (personnel is null)
    {
        return NotFound();
    }

    var assignmentHistory =
        await _assignmentService.GetByPersonnelIdAsync(id);

    var model = new PersonnelDetailsViewModel
    {
        Personnel = personnel,
        AssignmentHistory = assignmentHistory
    };

    return View(model);
}
}