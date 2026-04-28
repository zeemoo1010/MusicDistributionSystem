using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.Contracts.Services;

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

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult AccessDenied()
        {
            return View();
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
                if (result.RequiresVerification)
                {
                    ViewBag.PendingVerificationEmail = request.Email;
                }

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

            TempData["StatusMessage"] = $"We sent a verification code to {result.Email}. Enter the code to confirm your account.";
            return RedirectToAction(nameof(VerifyEmail), new { email = result.Email });
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult VerifyEmail(string? email = null)
        {
            return View(new VerifyEmailRequestDto
            {
                Email = email ?? string.Empty
            });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _accountService.VerifyEmailAsync(request);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Verification failed.");
                return View(request);
            }

            TempData["StatusMessage"] = "Your account has been verified. You can now log in.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> ResendVerificationCode(string email)
        {
            var result = await _accountService.ResendVerificationCodeAsync(email);
            TempData["StatusMessage"] = result.Succeeded
                ? $"A fresh verification code was sent to {email}."
                : result.ErrorMessage ?? "Unable to resend the verification code.";

            return RedirectToAction(nameof(VerifyEmail), new { email });
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordRequestDto());
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            await _accountService.RequestPasswordResetAsync(request);
            TempData["StatusMessage"] = "If the account exists, a password reset code has been sent to the email address.";
            return RedirectToAction(nameof(ResetPassword), new { email = request.Email });
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult ResetPassword(string? email = null)
        {
            return View(new ResetPasswordRequestDto
            {
                Email = email ?? string.Empty
            });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _accountService.ResetPasswordAsync(request);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Password reset failed.");
                return View(request);
            }

            TempData["StatusMessage"] = "Your password has been reset. Please log in with your new password.";
            return RedirectToAction(nameof(Login));
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
                new(ClaimTypes.Name, result.Username)
            };

            if (!string.IsNullOrWhiteSpace(result.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, result.Email));
            }

            foreach (var role in result.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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

