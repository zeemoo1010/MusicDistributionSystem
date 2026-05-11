using System.Security.Cryptography;
using MusicDistributionSystem.Domain.Constants;
using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Domain.Enums;
using MusicDistributionSystem.Infrastructure.Logging;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Application.Contracts.Repositories;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.Contracts.Security;

namespace MusicDistributionSystem.Application.Services
{
    public class AccountService : IAccountService
    {
        private const int VerificationCodeExpiryMinutes = 10;
        private const int PasswordResetCodeExpiryMinutes = 15;

        private readonly IUserRepository _userRepository;
        private readonly IAccountTokenRepository _accountTokenRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasherService _passwordHasherService;
        private readonly IAccountNotificationService _accountNotificationService;
        private readonly IAppLogger _appLogger;

        public AccountService(
            IUserRepository userRepository,
            IAccountTokenRepository accountTokenRepository,
            IRoleRepository roleRepository,
            IPasswordHasherService passwordHasherService,
            IAccountNotificationService accountNotificationService,
            IAppLogger appLogger)
        {
            _userRepository = userRepository;
            _accountTokenRepository = accountTokenRepository;
            _roleRepository = roleRepository;
            _passwordHasherService = passwordHasherService;
            _accountNotificationService = accountNotificationService;
            _appLogger = appLogger;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (existingUser is not null)
            {
                await _appLogger.LogWarningAsync("Account", $"Registration blocked for existing email '{normalizedEmail}'.");
                return new AuthResultDto
                {
                    ErrorMessage = "An account with this email already exists."
                };
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username.Trim(),
                Email = normalizedEmail,
                PasswordHash = _passwordHasherService.HashPassword(request.Password),
                MembershipTier = MembershipTier.Free,
                IsActive = false,
                IsEmailVerified = false
            };

            await _userRepository.AddAsync(user);
            var registeredUserRole = await _roleRepository.GetByNameAsync(RoleNames.User);
            if (registeredUserRole is not null)
            {
                await _userRepository.AddUserRoleAsync(new UserRole
                {
                    UserId = user.Id,
                    RoleId = registeredUserRole.Id
                });
            }

            await _userRepository.SaveChangesAsync();
            await SendFreshTokenAsync(user, AccountTokenType.EmailVerification);

            await _appLogger.LogInformationAsync("Account", $"New account registered for '{user.Email}' with pending verification.");

            return new AuthResultDto
            {
                Succeeded = true,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.UserRoles.Select(userRole => userRole.Role?.Name ?? string.Empty)
                    .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                    .ToArray(),
                RequiresVerification = true
            };
        }

        public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user is null || !user.IsActive)
            {
                await _appLogger.LogWarningAsync("Account", $"Failed login attempt for '{normalizedEmail}'.");
                return new AuthResultDto
                {
                    ErrorMessage = "Invalid email or password."
                };
            }

            if (!user.IsEmailVerified)
            {
                await _appLogger.LogWarningAsync("Account", $"Blocked login for unverified account '{normalizedEmail}'.");
                return new AuthResultDto
                {
                    ErrorMessage = "Please verify your email before logging in.",
                    RequiresVerification = true
                };
            }

            if (!_passwordHasherService.VerifyPassword(user.PasswordHash, request.Password))
            {
                await _appLogger.LogWarningAsync("Account", $"Invalid password attempt for '{normalizedEmail}'.");
                return new AuthResultDto
                {
                    ErrorMessage = "Invalid email or password."
                };
            }

            user.LastLoginAtUtc = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync();
            await _appLogger.LogInformationAsync("Account", $"Successful login for '{user.Email}'.");

            return new AuthResultDto
            {
                Succeeded = true,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.UserRoles.Select(userRole => userRole.Role?.Name ?? string.Empty)
                    .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                    .ToArray()
            };
        }

        public async Task<OperationResultDto> VerifyEmailAsync(VerifyEmailRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user is null)
            {
                return new OperationResultDto
                {
                    ErrorMessage = "We could not find an account for that email address."
                };
            }

            if (user.IsEmailVerified)
            {
                return new OperationResultDto
                {
                    Succeeded = true
                };
            }

            var token = await _accountTokenRepository.GetLatestActiveTokenAsync(user.Id, AccountTokenType.EmailVerification);
            if (token is null || !_passwordHasherService.VerifyPassword(token.TokenHash, request.Code.Trim()))
            {
                return new OperationResultDto
                {
                    ErrorMessage = "The verification code is invalid or has expired."
                };
            }

            token.ConsumedAtUtc = DateTime.UtcNow;
            user.IsEmailVerified = true;
            user.IsActive = true;

            await _accountTokenRepository.SaveChangesAsync();
            await _appLogger.LogInformationAsync("Account", $"Email verification completed for '{user.Email}'.");

            return new OperationResultDto
            {
                Succeeded = true
            };
        }

        public async Task RequestPasswordResetAsync(ForgotPasswordRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (user is null || !user.IsActive || !user.IsEmailVerified)
            {
                await _appLogger.LogWarningAsync("Account", $"Password reset requested for unknown or inactive email '{normalizedEmail}'.");
                return;
            }

            await SendFreshTokenAsync(user, AccountTokenType.PasswordReset);
            await _appLogger.LogInformationAsync("Account", $"Password reset issued for '{user.Email}'.");
        }

        public async Task<OperationResultDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user is null || !user.IsActive)
            {
                return new OperationResultDto
                {
                    ErrorMessage = "Unable to reset the password for this account."
                };
            }

            var token = await _accountTokenRepository.GetLatestActiveTokenAsync(user.Id, AccountTokenType.PasswordReset);
            if (token is null || !_passwordHasherService.VerifyPassword(token.TokenHash, request.Code.Trim()))
            {
                return new OperationResultDto
                {
                    ErrorMessage = "The reset code is invalid or has expired."
                };
            }

            user.PasswordHash = _passwordHasherService.HashPassword(request.Password);
            token.ConsumedAtUtc = DateTime.UtcNow;

            await _accountTokenRepository.SaveChangesAsync();
            await _appLogger.LogInformationAsync("Account", $"Password reset completed for '{user.Email}'.");

            return new OperationResultDto
            {
                Succeeded = true
            };
        }

        public async Task<OperationResultDto> ResendVerificationCodeAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user is null)
            {
                return new OperationResultDto
                {
                    ErrorMessage = "We could not find an account for that email address."
                };
            }

            if (user.IsEmailVerified)
            {
                return new OperationResultDto
                {
                    Succeeded = true
                };
            }

            await SendFreshTokenAsync(user, AccountTokenType.EmailVerification);
            await _appLogger.LogInformationAsync("Account", $"Verification code resent to '{user.Email}'.");

            return new OperationResultDto
            {
                Succeeded = true
            };
        }

        public async Task<IReadOnlyCollection<string>> GetUserRolesAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            if (user is null)
            {
                return Array.Empty<string>();
            }

            return user.UserRoles
                .Select(userRole => userRole.Role?.Name ?? string.Empty)
                .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                .ToArray();
        }

        private async Task SendFreshTokenAsync(User user, AccountTokenType tokenType)
        {
            var code = GenerateSixDigitCode();

            await _accountTokenRepository.InvalidateActiveTokensAsync(user.Id, tokenType);
            await _accountTokenRepository.AddAsync(new AccountToken
            {
                UserId = user.Id,
                Type = tokenType,
                Destination = user.Email,
                TokenHash = _passwordHasherService.HashPassword(code),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(
                    tokenType == AccountTokenType.EmailVerification
                        ? VerificationCodeExpiryMinutes
                        : PasswordResetCodeExpiryMinutes)
            });
            await _accountTokenRepository.SaveChangesAsync();

            if (tokenType == AccountTokenType.EmailVerification)
            {
                await _accountNotificationService.SendVerificationCodeAsync(user.Email, code);
                return;
            }

            await _accountNotificationService.SendPasswordResetCodeAsync(user.Email, code);
        }

        private static string GenerateSixDigitCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }
    }
}

