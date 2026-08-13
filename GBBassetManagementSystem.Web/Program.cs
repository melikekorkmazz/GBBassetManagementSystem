using GBBassetManagementSystem.Shared.Configuration;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

using GBBassetManagementSystem.Data.Context;
using GBBassetManagementSystem.Data.Seed;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Service.Interfaces;
using GBBassetManagementSystem.Service.Services;

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

// Application services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IPersonnelService, PersonnelService>();
builder.Services.AddScoped<IAssetAssignmentService, AssetAssignmentService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IRoomService, RoomService>();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(1);
});

builder.Services.AddMemoryCache();

// Localization resources are stored inside:
// GBBassetManagementSystem.Web/Resources
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+" +
            "çÇğĞıİöÖşŞüÜ";

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Authentication cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.Cookie.IsEssential = true;
});

builder.Services
    .AddControllersWithViews(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        options.Filters.Add(
            new AuthorizeFilter(policy));
    })
    .AddViewLocalization()
   .AddDataAnnotationsLocalization(options =>
{
    options.DataAnnotationLocalizerProvider =
        (type, factory) =>
            factory.Create(
                typeof(GBBassetManagementSystem.Web.SharedResource));
});

// Supported languages
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("tr")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture =
        new RequestCulture("en");

    options.SupportedCultures =
        supportedCultures;

    options.SupportedUICultures =
        supportedCultures;

    // The culture cookie has priority over browser language.
    options.RequestCultureProviders =
    [
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Localization must run before authentication,
// authorization and controller execution.
app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Creates initial roles and administrator account.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager =
        services.GetRequiredService<
            UserManager<ApplicationUser>>();

    var roleManager =
        services.GetRequiredService<
            RoleManager<IdentityRole>>();

    await IdentitySeeder.SeedAsync(
        userManager,
        roleManager);
}

app.Run();