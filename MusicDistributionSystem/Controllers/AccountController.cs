using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.DTOs.Account;
using MusicDistributionSystem.Services.Interfaces;

namespace MusicDistributionSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult Login()
        {
            return View(new LoginRequestDto());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _accountService.LoginAsync(request);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Login failed.");
                return View(request);
            }

            await SignInAsync(result, request.RememberMe);
            TempData["StatusMessage"] = $"Welcome back, {result.Username}.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult Register()
        {
            return View(new RegisterRequestDto());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _accountService.RegisterAsync(request);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Registration failed.");
                return View(request);
            }

            await SignInAsync(result, isPersistent: false);
            TempData["StatusMessage"] = $"Welcome to SoundSphere, {result.Username}.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["StatusMessage"] = "You have been signed out.";
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInAsync(AuthResultDto result, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                new(ClaimTypes.Name, result.Username),
                new(ClaimTypes.Email, result.Email),
                new(ClaimTypes.Role, result.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = isPersistent,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(isPersistent ? 14 : 1)
                });
        }
    }
}
