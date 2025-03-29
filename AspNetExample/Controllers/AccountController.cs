using AspNetExample.Common.Extensions;
using AspNetExample.Domain.Entities;
using AspNetExample.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetExample.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [AllowAnonymous]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        return View(new LoginModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Login, model.Password, false, false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(model.Login), "Некорректные логин и(или) пароль");
            return View(model);
        }

        if (model.ReturnUrl.IsSignificant())
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register([FromQuery] string? returnUrl = null)
    {
        return View(new RegisterModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([FromForm] RegisterModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new User { Id = Guid.NewGuid(), Name = model.Login };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(nameof(model.Login), error.Description);

            return View(model);
        }

        await _signInManager.SignInAsync(user, false);

        if (model.ReturnUrl.IsSignificant())
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}