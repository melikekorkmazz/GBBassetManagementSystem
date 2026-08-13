using System.Text;
using GBBassetManagementSystem.Service.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using GBBassetManagementSystem.Entity.Entities;
using GBBassetManagementSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Caching.Memory;


namespace GBBassetManagementSystem.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly IEmailService _emailService;
    private readonly IMemoryCache _memoryCache;

    // Injects the Identity and localization services.
public AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailService emailService,
    IStringLocalizer<SharedResource> localizer,
    IMemoryCache memoryCache)
{
    _userManager = userManager;
    _signInManager = signInManager;
    _emailService = emailService;
    _localizer = localizer;
    _memoryCache = memoryCache;
}

    // Displays the login page.
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        LoginViewModel model = new()
        {
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    // Processes the login form.
    [AllowAnonymous]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(LoginViewModel model)
{
    // Replace default validation messages with localized messages.
    if (string.IsNullOrWhiteSpace(model.UsernameOrEmail))
    {
        ModelState.Remove(nameof(model.UsernameOrEmail));

        ModelState.AddModelError(
            nameof(model.UsernameOrEmail),
            _localizer["UsernameOrEmailRequired"].Value);
    }

    if (string.IsNullOrWhiteSpace(model.Password))
    {
        ModelState.Remove(nameof(model.Password));

        ModelState.AddModelError(
            nameof(model.Password),
            _localizer["PasswordRequired"].Value);
    }

    if (!ModelState.IsValid)
    {
        return View(model);
    }

    ApplicationUser? user;

    string usernameOrEmail = model.UsernameOrEmail.Trim();

    // Finds the user by email or username.
    if (usernameOrEmail.Contains('@'))
    {
        user = await _userManager.FindByEmailAsync(usernameOrEmail);
    }
    else
    {
        user = await _userManager.FindByNameAsync(usernameOrEmail);
    }

    if (user is null)
    {
        ModelState.AddModelError(
            string.Empty,
            _localizer["InvalidLoginAttempt"].Value);

        return View(model);
    }

    IdentitySignInResult result =
        await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

    if (result.Succeeded)
    {
        // Redirects the user to the originally requested page.
        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
            Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    if (result.IsLockedOut)
    {
        ModelState.AddModelError(
            string.Empty,
            _localizer["AccountTemporarilyLocked"].Value);

        return View(model);
    }

    if (result.IsNotAllowed)
    {
        ModelState.AddModelError(
            string.Empty,
            _localizer["AccountNotAllowed"].Value);

        return View(model);
    }

    ModelState.AddModelError(
        string.Empty,
        _localizer["InvalidLoginAttempt"].Value);

    return View(model);
}
    // Signs the current user out of the application.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        TempData["SuccessMessage"] =
            _localizer["LogoutSuccessful"].Value;

        return RedirectToAction(nameof(Login));
    }

    // Displays the access denied page.
    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
   // Displays the forgot password page.
[AllowAnonymous]
[HttpGet]
public IActionResult ForgotPassword()
{
    return View(new ForgotPasswordViewModel());
}

// Generates and sends the password reset link.
[AllowAnonymous]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ForgotPassword(
    ForgotPasswordViewModel model)
{
    string cacheKey = $"ForgotPassword_{model.Email}";

if (_memoryCache.TryGetValue(cacheKey, out _))
{
    TempData["ErrorMessage"] =
        _localizer["ResetPasswordRecentlyRequested"].Value;

    TempData["WaitSeconds"] = 60;

    return RedirectToAction(nameof(ForgotPassword));
}
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    string emailAddress = model.Email.Trim();

    ApplicationUser? user =
        await _userManager.FindByEmailAsync(emailAddress);

    // Do not reveal whether the email exists.
    if (user is null)
    {
        return RedirectToAction(
            nameof(ForgotPasswordConfirmation));
    }

    string token =
        await _userManager.GeneratePasswordResetTokenAsync(user);

    string encodedToken =
        WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token));

    string? resetUrl = Url.Action(
        nameof(ResetPassword),
        "Account",
        new
        {
            email = user.Email,
            token = encodedToken
        },
        Request.Scheme);

    if (string.IsNullOrWhiteSpace(resetUrl))
    {
        ModelState.AddModelError(
            string.Empty,
            _localizer["PasswordResetLinkCouldNotBeCreated"]);

        return View(model);
    }

    string subject =
        _localizer["PasswordResetEmailSubject"].Value;

  string message = $"""
    <div style="font-family: Arial, sans-serif; line-height: 1.6;">
        <h2>{_localizer["PasswordResetEmailTitle"]}</h2>

        <p>
            {_localizer["PasswordResetEmailDescription"]}
        </p>

        <p>
            <a href="{resetUrl}"
               style="
                   display: inline-block;
                   padding: 12px 20px;
                   background-color: #696cff;
                   color: white;
                   text-decoration: none;
                   border-radius: 6px;">
                {_localizer["ResetPassword"]}
            </a>
        </p>

        <p>
            {_localizer["PasswordResetEmailIgnoreMessage"]}
        </p>

        <hr style="margin:20px 0;border:none;border-top:1px solid #ddd;" />

        <p style="font-size:14px;color:#6c757d;">
            ⏰ {_localizer["PasswordResetLinkExpiration"]}
        </p>
    </div>
    """;
    await _emailService.SendEmailAsync(
        user.Email!,
        subject,
        message);
        _memoryCache.Set(
    cacheKey,
    true,
    TimeSpan.FromMinutes(1));

    return RedirectToAction(
        nameof(ForgotPasswordConfirmation));
}
// Displays the email sent confirmation page.
[AllowAnonymous]
[HttpGet]
public IActionResult ForgotPasswordConfirmation()
{
    return View();
}
[AllowAnonymous]
[HttpGet]
public async Task<IActionResult> ResetPassword(
    string? email,
    string? token)
{
    if (string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(token))
    {
        TempData["ErrorMessage"] =
            _localizer["InvalidResetPasswordLink"].Value;

        return RedirectToAction(nameof(Login));
    }

    ApplicationUser? user =
        await _userManager.FindByEmailAsync(email);

    if (user is null)
    {
        TempData["ErrorMessage"] =
            _localizer["InvalidResetPasswordLink"].Value;

        return RedirectToAction(nameof(Login));
    }

    try
    {
        string decodedToken =
            Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(token));

        bool isTokenValid =
            await _userManager.VerifyUserTokenAsync(
                user,
                _userManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<ApplicationUser>.ResetPasswordTokenPurpose,
                decodedToken);

        if (!isTokenValid)
        {
            TempData["ErrorMessage"] =
                _localizer["ResetPasswordLinkExpired"].Value;

            return RedirectToAction(nameof(Login));
        }
    }
    catch
    {
        TempData["ErrorMessage"] =
            _localizer["InvalidResetPasswordLink"].Value;

        return RedirectToAction(nameof(Login));
    }

    ResetPasswordViewModel model = new()
    {
        Email = email,
        Token = token
    };

    return View(model);
}
[AllowAnonymous]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ResetPassword(
    ResetPasswordViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var user = await _userManager.FindByEmailAsync(model.Email);

    // Do not reveal whether the email address exists.
    if (user == null)
    {
        return RedirectToAction(
            nameof(ResetPasswordConfirmation));
    }

    string decodedToken;

    try
    {
        decodedToken = Encoding.UTF8.GetString(
            WebEncoders.Base64UrlDecode(model.Token));
    }
    catch
    {
        ModelState.AddModelError(
            string.Empty,
            _localizer["ResetPasswordLinkExpired"].Value);

        return View(model);
    }

    IdentityResult result =
        await _userManager.ResetPasswordAsync(
            user,
            decodedToken,
            model.Password);

    if (result.Succeeded)
    {
        return RedirectToAction(
            nameof(ResetPasswordConfirmation));
    }

    foreach (IdentityError error in result.Errors)
    {
        string errorMessage = error.Code switch
        {
            "InvalidToken" =>
                _localizer["ResetPasswordLinkExpired"].Value,

            "PasswordTooShort" =>
                _localizer["PasswordTooShort"].Value,

            "PasswordRequiresNonAlphanumeric" =>
                _localizer["PasswordRequiresSpecialCharacter"].Value,

            "PasswordRequiresDigit" =>
                _localizer["PasswordRequiresDigit"].Value,

            "PasswordRequiresUpper" =>
                _localizer["PasswordRequiresUppercase"].Value,

            "PasswordRequiresLower" =>
                _localizer["PasswordRequiresLowercase"].Value,

            _ =>
                _localizer["PasswordResetFailed"].Value
        };

        ModelState.AddModelError(
            string.Empty,
            errorMessage);
    }

    return View(model);
}
[AllowAnonymous]
[HttpGet]
public IActionResult ResetPasswordConfirmation()
{
    return View();
}
}