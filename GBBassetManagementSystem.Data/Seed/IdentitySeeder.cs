using GBBassetManagementSystem.Entity.Entities;
using Microsoft.AspNetCore.Identity;

namespace GBBassetManagementSystem.Data.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        string[] roles =
        {
            "Admin",
            "DepartmentUser"
        };

        // Creates the required roles if they do not already exist.
        foreach (string role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "ruby@gbb.com";
        const string adminPassword = "Ruby_4950";

        ApplicationUser? adminUser =
            await userManager.FindByEmailAsync(adminEmail);

        // Creates the first administrator account.
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "Ruby",
                Email = adminEmail,
                FirstName = "Melike",
                LastName = "Korkmaz",
                EmailConfirmed = true,
                DepartmentId = null
            };

            IdentityResult result =
                await userManager.CreateAsync(adminUser, adminPassword);

            if (!result.Succeeded)
            {
                string errors = string.Join(
                    ", ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"The administrator account could not be created: {errors}");
            }
        }

        // Assigns the Admin role if the user does not already have it.
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}