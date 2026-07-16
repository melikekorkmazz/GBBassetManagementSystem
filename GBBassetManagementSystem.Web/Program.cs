using GBBassetManagementSystem.Service.Interfaces;
using GBBassetManagementSystem.Service.Services;

using GBBassetManagementSystem.Data.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IDepartmentService, DepartmentService>();
    builder.Services.AddScoped<IPersonnelService, PersonnelService>();
    builder.Services.AddScoped<IAssetAssignmentService, AssetAssignmentService>();
    builder.Services.AddScoped<IAssetService, AssetService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();