using FluentAssertions;
using Moq;
using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.Services;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Domain.Contracts.Logging;
using MusicDistributionSystem.Domain.Contracts.Security;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Tests.Services;

public class AccountServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IAccountTokenRepository> _tokenRepo = new();
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IPasswordHasherService> _hasher = new();
    private readonly Mock<IAccountNotificationService> _notifier = new();
    private readonly Mock<IAppLogger> _logger = new();
    private readonly AccountService _sut;

    public AccountServiceTests()
    {
        _hasher.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("hashed");
        _sut = new AccountService(_userRepo.Object, _tokenRepo.Object, _roleRepo.Object,
            _hasher.Object, _notifier.Object, _logger.Object);
    }

    private static User MakeUser(bool verified = false, bool active = false)
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@test.com",
            PasswordHash = "hashed",
            IsEmailVerified = verified,
            IsActive = active,
            MembershipTier = MembershipTier.Free
        };
        return user;
    }

    // ── RegisterAsync ──

    [Fact]
    public async Task RegisterAsync_WhenEmailTakenButUnverified_ShouldResendCode()
    {
        var existing = MakeUser(verified: false);
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(existing);

        var dto = new RegisterRequestDto { Email = "test@test.com", Password = "Pass1234!", Username = "taken" };
        var result = await _sut.RegisterAsync(dto);

        result.Succeeded.Should().BeTrue();
        result.RequiresVerification.Should().BeTrue();
        _notifier.Verify(n => n.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailTakenAndVerified_ShouldReturnError()
    {
        var existing = MakeUser(verified: true, active: true);
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(existing);

        var dto = new RegisterRequestDto { Email = "test@test.com", Password = "Pass1234!", Username = "taken" };
        var result = await _sut.RegisterAsync(dto);

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task RegisterAsync_WhenNewUser_ShouldRegisterAndReturnRequiresVerification()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("new@test.com")).ReturnsAsync((User?)null);
        _roleRepo.Setup(r => r.GetByNameAsync("User")).ReturnsAsync(new Role { Name = "User" });

        var dto = new RegisterRequestDto { Email = "new@test.com", Password = "Pass1234!", Username = "newuser" };
        var result = await _sut.RegisterAsync(dto);

        result.Succeeded.Should().BeTrue();
        result.RequiresVerification.Should().BeTrue();
        result.Email.Should().Be("new@test.com");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepo.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
    }

    // ── LoginAsync ──

    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ShouldReturnError()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("nope@test.com")).ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(new LoginRequestDto { Email = "nope@test.com", Password = "x" });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WhenUserInactive_ShouldReturnError()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(new User { IsActive = false, PasswordHash = "x" });

        var result = await _sut.LoginAsync(new LoginRequestDto { Email = "test@test.com", Password = "x" });

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_WhenEmailNotVerified_ShouldReturnRequiresVerification()
    {
        var user = MakeUser(verified: false, active: true);
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);

        var result = await _sut.LoginAsync(new LoginRequestDto { Email = "test@test.com", Password = "x" });

        result.Succeeded.Should().BeFalse();
        result.RequiresVerification.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordWrong_ShouldReturnError()
    {
        var user = MakeUser(verified: true, active: true);
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        _hasher.Setup(h => h.VerifyPassword("hashed", "wrongpass")).Returns(false);

        var result = await _sut.LoginAsync(new LoginRequestDto { Email = "test@test.com", Password = "wrongpass" });

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_WhenValid_ShouldSucceed()
    {
        var user = MakeUser(verified: true, active: true);
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        _hasher.Setup(h => h.VerifyPassword("hashed", "Pass1234!")).Returns(true);

        var result = await _sut.LoginAsync(new LoginRequestDto { Email = "test@test.com", Password = "Pass1234!" });

        result.Succeeded.Should().BeTrue();
        result.Email.Should().Be("test@test.com");
    }

    // ── VerifyEmailAsync ──

    [Fact]
    public async Task VerifyEmailAsync_WhenUserNotFound_ShouldReturnError()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("nope@test.com")).ReturnsAsync((User?)null);

        var result = await _sut.VerifyEmailAsync(new VerifyEmailRequestDto { Email = "nope@test.com", Code = "123456" });

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenInvalidCode_ShouldReturnError()
    {
        var user = MakeUser(verified: false);
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        _tokenRepo.Setup(r => r.GetLatestActiveTokenAsync(user.Id, AccountTokenType.EmailVerification))
            .ReturnsAsync(new AccountToken { TokenHash = "hashed-code" });
        _hasher.Setup(h => h.VerifyPassword("hashed-code", "000000")).Returns(false);

        var result = await _sut.VerifyEmailAsync(new VerifyEmailRequestDto { Email = "test@test.com", Code = "000000" });

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("invalid");
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenValidCode_ShouldVerify()
    {
        var user = MakeUser(verified: false);
        _userRepo.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);
        _tokenRepo.Setup(r => r.GetLatestActiveTokenAsync(user.Id, AccountTokenType.EmailVerification))
            .ReturnsAsync(new AccountToken { TokenHash = "hashed-code" });
        _hasher.Setup(h => h.VerifyPassword("hashed-code", "123456")).Returns(true);

        var result = await _sut.VerifyEmailAsync(new VerifyEmailRequestDto { Email = "test@test.com", Code = "123456" });

        result.Succeeded.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
        user.IsActive.Should().BeTrue();
    }
}
