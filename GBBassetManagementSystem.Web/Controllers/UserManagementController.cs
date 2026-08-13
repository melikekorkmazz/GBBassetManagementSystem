using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using GBBassetManagementSystem.Web.Models;
using Microsoft.Extensions.Localization;

namespace GBBassetManagementSystem.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDepartmentService _departmentService;
    private readonly IStringLocalizer<SharedResource> _localizer;
   public UserManagementController(
    UserManager<ApplicationUser> userManager,
    IDepartmentService departmentService,
    IStringLocalizer<SharedResource> localizer)
{
    _userManager = userManager;
    _departmentService = departmentService;
    _localizer = localizer;
}
[HttpGet]
public async Task<IActionResult> Index()
{
    var users = await _userManager.Users
        .AsNoTracking()
        .Where(user => !user.IsArchived)
        .Include(user => user.Department)
        .OrderBy(user => user.FirstName)
        .ThenBy(user => user.LastName)
        .ToListAsync();

    var model = new List<UserListViewModel>();

    foreach (var user in users)
    {
        var roles =
            await _userManager.GetRolesAsync(user);

        model.Add(new UserListViewModel
        {
            Id = user.Id,

            FullName =
                $"{user.FirstName} {user.LastName}".Trim(),

            UserName =
                user.UserName ?? string.Empty,

            Email =
                user.Email ?? string.Empty,

            DepartmentName =
                user.Department?.Name ??
                "All Departments",

            RoleName =
                roles.FirstOrDefault() ??
                "No Role"
        });
    }

    return View(model);
}
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreateUserViewModel();

        await LoadDepartmentsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateUserViewModel model)
    {
        NormalizeModel(model);

        if (!model.DepartmentId.HasValue ||
            model.DepartmentId.Value == Guid.Empty)
        {
            ModelState.AddModelError(
                nameof(model.DepartmentId),
                "Please select a department.");
        }
        else
        {
            var department =
                await _departmentService.GetByIdAsync(
                    model.DepartmentId.Value);

            if (department is null)
            {
                ModelState.AddModelError(
                    nameof(model.DepartmentId),
                    "The selected department was not found.");
            }
        }

        if (!string.IsNullOrWhiteSpace(model.UserName))
        {
            var existingUserByName =
                await _userManager.FindByNameAsync(
                    model.UserName);

            if (existingUserByName is not null)
            {
                ModelState.AddModelError(
                    nameof(model.UserName),
                    "This username is already in use.");
            }
        }

        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            var existingUserByEmail =
                await _userManager.FindByEmailAsync(
                    model.Email);

            if (existingUserByEmail is not null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email address is already in use.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(model);

            return View(model);
        }

        var user = new ApplicationUser
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            UserName = model.UserName,
            Email = model.Email,
            DepartmentId = model.DepartmentId
        };

        var createResult =
            await _userManager.CreateAsync(
                user,
                model.Password);

        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);

            await LoadDepartmentsAsync(model);

            return View(model);
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                "DepartmentUser");

        if (!roleResult.Succeeded)
        {
            // Prevent an incomplete account from remaining
            // in the database when role assignment fails.
            await _userManager.DeleteAsync(user);

            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"The user could not be assigned to the " +
                    $"DepartmentUser role: {error.Description}");
            }

            await LoadDepartmentsAsync(model);

            return View(model);
        }

        TempData["SuccessMessage"] =
            $"{user.FirstName} {user.LastName} " +
            "was created successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDepartmentsAsync(
        CreateUserViewModel model)
    {
        var departments =
            await _departmentService.GetAllAsync();

        model.Departments = departments
            .OrderBy(department => department.Name)
            .Select(department =>
                new SelectListItem
                {
                    Text = department.Name,
                    Value = department.Id.ToString(),
                    Selected =
                        model.DepartmentId ==
                        department.Id
                })
            .ToList();
    }

    private static void NormalizeModel(
        CreateUserViewModel model)
    {
        model.FirstName =
            model.FirstName?.Trim() ?? string.Empty;

        model.LastName =
            model.LastName?.Trim() ?? string.Empty;

        model.UserName =
            model.UserName?.Trim() ?? string.Empty;

        model.Email =
            model.Email?.Trim() ?? string.Empty;
    }

    private void AddIdentityErrors(
        IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                error.Description);
        }
    }


    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Archive(string id)
{
    if (string.IsNullOrWhiteSpace(id))
    {
        TempData["ErrorMessage"] =
            "The selected user was not found.";

        return RedirectToAction(nameof(Index));
    }

    var user =
        await _userManager.FindByIdAsync(id);

    if (user is null)
    {
        TempData["ErrorMessage"] =
            "The selected user was not found.";

        return RedirectToAction(nameof(Index));
    }

    var roles =
        await _userManager.GetRolesAsync(user);

    if (roles.Contains("Admin"))
    {
        TempData["ErrorMessage"] =
            "Administrator accounts cannot be archived.";

        return RedirectToAction(nameof(Index));
    }

    user.IsArchived = true;
    user.ArchivedDate = DateTime.UtcNow;

    // Prevent the archived account from signing in.
    user.LockoutEnabled = true;
    user.LockoutEnd = DateTimeOffset.MaxValue;

    var result =
        await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
    {
        TempData["ErrorMessage"] =
            "The user could not be archived.";

        return RedirectToAction(nameof(Index));
    }

    await _userManager.UpdateSecurityStampAsync(user);

    TempData["SuccessMessage"] =
        $"{user.FirstName} {user.LastName} was archived successfully.";

    return RedirectToAction(nameof(Index));
}
[HttpGet]
public async Task<IActionResult> ArchivedUsers()
{
    var users = await _userManager.Users
        .AsNoTracking()
        .Where(user => user.IsArchived)
        .Include(user => user.Department)
        .OrderByDescending(user => user.ArchivedDate)
        .ToListAsync();

    var model = new List<UserListViewModel>();

    foreach (var user in users)
    {
        var roles =
            await _userManager.GetRolesAsync(user);

        model.Add(new UserListViewModel
        {
            Id = user.Id,

            FullName =
                $"{user.FirstName} {user.LastName}".Trim(),

            UserName =
                user.UserName ?? string.Empty,

            Email =
                user.Email ?? string.Empty,

            DepartmentName =
                user.Department?.Name ??
                "All Departments",

            RoleName =
                roles.FirstOrDefault() ??
                "No Role"
        });
    }

    return View(model);
}
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Activate(string id)
{
    if (string.IsNullOrWhiteSpace(id))
    {
        TempData["ErrorMessage"] =
            "The selected user was not found.";

        return RedirectToAction(nameof(ArchivedUsers));
    }

    var user =
        await _userManager.FindByIdAsync(id);

    if (user is null)
    {
        TempData["ErrorMessage"] =
            "The selected user was not found.";

        return RedirectToAction(nameof(ArchivedUsers));
    }

    user.IsArchived = false;
    user.ArchivedDate = null;

    user.LockoutEnd = null;

    var result =
        await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
    {
        TempData["ErrorMessage"] =
            "The user could not be activated.";

        return RedirectToAction(nameof(ArchivedUsers));
    }

    await _userManager.UpdateSecurityStampAsync(user);

    TempData["SuccessMessage"] =
        $"{user.FirstName} {user.LastName} was activated successfully.";

    return RedirectToAction(nameof(ArchivedUsers));
}
}