using MusicDistributionSystem.DTOs.Account;
using MusicDistributionSystem.Enums;
using MusicDistributionSystem.Models;
using MusicDistributionSystem.Repositories.Interfaces;
using MusicDistributionSystem.Services.Interfaces;
using MusicDistributionSystem.Services.Security;

namespace MusicDistributionSystem.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherService _passwordHasherService;

        public AccountService(IUserRepository userRepository, IPasswordHasherService passwordHasherService)
        {
            _userRepository = userRepository;
            _passwordHasherService = passwordHasherService;
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (existingUser is not null)
            {
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
                Role = "User",
                MembershipTier = MembershipTier.Free,
                IsActive = true,
                IsEmailVerified = false
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResultDto
            {
                Succeeded = true,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail);

            if (user is null || !user.IsActive)
            {
                return new AuthResultDto
                {
                    ErrorMessage = "Invalid email or password."
                };
            }

            if (!_passwordHasherService.VerifyPassword(user.PasswordHash, request.Password))
            {
                return new AuthResultDto
                {
                    ErrorMessage = "Invalid email or password."
                };
            }

            return new AuthResultDto
            {
                Succeeded = true,
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}
