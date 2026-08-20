using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using MusicDistributionSystem.Domain.Contracts.Logging;
using MusicDistributionSystem.Infrastructure.Configuration;
using MusicDistributionSystem.Infrastructure.Notifications;

namespace MusicDistributionSystem.Tests.Services;

public class AccountNotificationServiceTests
{
    private readonly Mock<IAppLogger> _logger = new();

    [Fact]
    public async Task SendVerificationCodeAsync_WhenSmtpNotConfigured_ShouldLogWarningAndNotThrow()
    {
        var settings = Options.Create(new EmailSettings()); // Unconfigured (empty)
        var sut = new AccountNotificationService(settings, _logger.Object);

        var act = async () => await sut.SendVerificationCodeAsync("test@example.com", "123456");

        await act.Should().NotThrowAsync();
        _logger.Verify(l => l.LogWarningAsync("AccountNotification", It.Is<string>(s => s.Contains("SMTP not configured"))), Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetCodeAsync_WhenSmtpNotConfigured_ShouldLogWarningAndNotThrow()
    {
        var settings = Options.Create(new EmailSettings());
        var sut = new AccountNotificationService(settings, _logger.Object);

        var act = async () => await sut.SendPasswordResetCodeAsync("test@example.com", "654321");

        await act.Should().NotThrowAsync();
        _logger.Verify(l => l.LogWarningAsync("AccountNotification", It.Is<string>(s => s.Contains("SMTP not configured"))), Times.Once);
    }

    [Fact]
    public async Task SendPaymentReceiptAsync_WhenSmtpNotConfigured_ShouldLogWarningAndNotThrow()
    {
        var settings = Options.Create(new EmailSettings());
        var sut = new AccountNotificationService(settings, _logger.Object);

        var act = async () => await sut.SendPaymentReceiptAsync("test@example.com", "Premium Pulse", 2500, "MDS-REF-123");

        await act.Should().NotThrowAsync();
        _logger.Verify(l => l.LogWarningAsync("AccountNotification", It.Is<string>(s => s.Contains("SMTP not configured"))), Times.Once);
    }
}
