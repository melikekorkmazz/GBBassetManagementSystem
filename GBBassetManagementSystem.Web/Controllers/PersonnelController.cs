using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using GBBassetManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GBBassetManagementSystem.Web.Controllers;

[Authorize(Roles = "Admin,DepartmentUser")]
public class PersonnelController : Controller
{
    private readonly IPersonnelService _personnelService;
    private readonly IDepartmentService _departmentService;
    private readonly IAssetAssignmentService _assignmentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PersonnelController(
        IPersonnelService personnelService,
        IDepartmentService departmentService,
        IAssetAssignmentService assignmentService,
        UserManager<ApplicationUser> userManager)
    {
        _personnelService = personnelService;
        _departmentService = departmentService;
        _assignmentService = assignmentService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var personnel = await _personnelService.GetAllAsync();

        if (User.IsInRole("DepartmentUser"))
        {
            var departmentId = await GetCurrentDepartmentIdAsync();

            if (departmentId is null)
            {
                return Forbid();
            }

            personnel = personnel
                .Where(person =>
                    person.DepartmentId == departmentId.Value)
                .ToList();
        }

        return View(personnel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (User.IsInRole("Admin"))
        {
            await LoadDepartmentsAsync();
        }
        else
        {
            var departmentId = await GetCurrentDepartmentIdAsync();

            if (departmentId is null)
            {
                return Forbid();
            }

            await LoadDepartmentsAsync(departmentId);
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Personnel personnel)
    {
        if (User.IsInRole("DepartmentUser"))
        {
            var departmentId = await GetCurrentDepartmentIdAsync();

            if (departmentId is null)
            {
                return Forbid();
            }

            // Prevents DepartmentUser from selecting or posting
            // another department manually.
            personnel.DepartmentId = departmentId.Value;

            ModelState.Remove(nameof(Personnel.DepartmentId));
        }

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsForCurrentUserAsync(
                personnel.DepartmentId);

            return View(personnel);
        }

        await _personnelService.AddAsync(personnel);

        TempData["SuccessMessage"] =
            "Personnel saved successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var personnel =
            await _personnelService.GetByIdAsync(id);

        if (personnel is null)
        {
            return NotFound();
        }

        if (!await CanAccessPersonnelAsync(personnel))
        {
            return Forbid();
        }

        await LoadDepartmentsForCurrentUserAsync(
            personnel.DepartmentId);

        return View(personnel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        Personnel personnel)
    {
        if (id != personnel.Id)
        {
            return BadRequest();
        }

        var existingPersonnel =
            await _personnelService.GetByIdAsync(id);

        if (existingPersonnel is null)
        {
            return NotFound();
        }

        if (!await CanAccessPersonnelAsync(existingPersonnel))
        {
            return Forbid();
        }

        if (User.IsInRole("DepartmentUser"))
        {
            var departmentId = await GetCurrentDepartmentIdAsync();

            if (departmentId is null)
            {
                return Forbid();
            }

            // Prevents changing the department through form tampering.
            personnel.DepartmentId = departmentId.Value;

            ModelState.Remove(nameof(Personnel.DepartmentId));
        }

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsForCurrentUserAsync(
                personnel.DepartmentId);

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

        TempData["SuccessMessage"] =
            "Personnel updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var personnel =
            await _personnelService.GetByIdAsync(id);

        if (personnel is null)
        {
            return NotFound();
        }

        if (!await CanAccessPersonnelAsync(personnel))
        {
            return Forbid();
        }

        return View(personnel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var personnel =
            await _personnelService.GetByIdAsync(id);

        if (personnel is null)
        {
            return NotFound();
        }

        if (!await CanAccessPersonnelAsync(personnel))
        {
            return Forbid();
        }

        try
        {
            await _personnelService.DeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] =
            "Personnel deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var personnel =
            await _personnelService.GetByIdAsync(id);

        if (personnel is null)
        {
            return NotFound();
        }

        if (!await CanAccessPersonnelAsync(personnel))
        {
            return Forbid();
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

    private async Task<Guid?> GetCurrentDepartmentIdAsync()
    {
        var currentUser =
            await _userManager.GetUserAsync(User);

        return currentUser?.DepartmentId;
    }

    private async Task<bool> CanAccessPersonnelAsync(
        Personnel personnel)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var departmentId =
            await GetCurrentDepartmentIdAsync();

        return departmentId.HasValue &&
               personnel.DepartmentId == departmentId.Value;
    }

    private async Task LoadDepartmentsForCurrentUserAsync(
        Guid? selectedDepartmentId = null)
    {
        if (User.IsInRole("Admin"))
        {
            await LoadDepartmentsAsync(selectedDepartmentId);
            return;
        }

        var departmentId =
            await GetCurrentDepartmentIdAsync();

        if (departmentId.HasValue)
        {
            await LoadDepartmentsAsync(departmentId.Value);
        }
    }

    private async Task LoadDepartmentsAsync(
        Guid? selectedDepartmentId = null)
    {
        var departments =
            await _departmentService.GetAllAsync();

        if (User.IsInRole("DepartmentUser"))
        {
            var departmentId =
                await GetCurrentDepartmentIdAsync();

            departments = departmentId.HasValue
                ? departments
                    .Where(department =>
                        department.Id == departmentId.Value)
                    .ToList()
                : [];
        }

        ViewBag.Departments = new SelectList(
            departments,
            "Id",
            "Name",
            selectedDepartmentId);
    }
}