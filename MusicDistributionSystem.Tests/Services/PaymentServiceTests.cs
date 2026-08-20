using FluentAssertions;
using Moq;
using MusicDistributionSystem.Application.Contracts.Payments;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Payment;
using MusicDistributionSystem.Application.Services;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Contracts.Logging;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IMembershipPlanRepository> _planRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPaymentTransactionRepository> _txRepo = new();
    private readonly Mock<IUserSubscriptionRepository> _subRepo = new();
    private readonly Mock<IPaymentGateway> _gateway = new();
    private readonly Mock<IAccountNotificationService> _notifier = new();
    private readonly Mock<IAppLogger> _logger = new();
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _sut = new PaymentService(
            _planRepo.Object,
            _userRepo.Object,
            _txRepo.Object,
            _subRepo.Object,
            _gateway.Object,
            _notifier.Object,
            _logger.Object);
    }

    private static MembershipPlan MakePlan(decimal price = 2500, MembershipTier tier = MembershipTier.Premium) => new()
    {
        Name = "Premium Pulse",
        Tier = tier,
        MonthlyPrice = price,
        MonthlyDownloadLimit = 250
    };

    private static User MakeUser(bool active = true, bool verified = true) => new()
    {
        Username = "testuser",
        Email = "test@test.com",
        PasswordHash = "hash",
        IsActive = active,
        IsEmailVerified = verified,
        MembershipTier = MembershipTier.Free
    };

    // ── GetCheckoutAsync ──

    [Fact]
    public async Task GetCheckoutAsync_WhenPlanExists_ShouldReturnCheckout()
    {
        var plan = MakePlan();
        _planRepo.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);

        var result = await _sut.GetCheckoutAsync(plan.Id);

        result.Should().NotBeNull();
        result!.PlanName.Should().Be(plan.Name);
        result.MonthlyPrice.Should().Be(plan.MonthlyPrice);
    }

    [Fact]
    public async Task GetCheckoutAsync_WhenPlanMissing_ShouldReturnNull()
    {
        _planRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((MembershipPlan?)null);

        var result = await _sut.GetCheckoutAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── InitializePaymentAsync ──

    [Fact]
    public async Task InitializePaymentAsync_WhenPlanMissing_ShouldReturnError()
    {
        _planRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((MembershipPlan?)null);

        var result = await _sut.InitializePaymentAsync(Guid.NewGuid(), Guid.NewGuid(), "a@b.com", "url");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("plan");
    }

    [Fact]
    public async Task InitializePaymentAsync_WhenUserInvalid_ShouldReturnError()
    {
        var plan = MakePlan();
        _planRepo.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var result = await _sut.InitializePaymentAsync(plan.Id, Guid.NewGuid(), "a@b.com", "url");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("active");
    }

    [Fact]
    public async Task InitializePaymentAsync_WhenFreePlan_ShouldActivateWithoutGateway()
    {
        var plan = MakePlan(price: 0, tier: MembershipTier.Free);
        var user = MakeUser();
        _planRepo.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _subRepo.Setup(r => r.GetActiveByUserAsync(user.Id)).ReturnsAsync((UserSubscription?)null);

        var result = await _sut.InitializePaymentAsync(plan.Id, user.Id, user.Email, "url");

        result.Succeeded.Should().BeTrue();
        result.RequiresRedirect.Should().BeFalse();
        user.MembershipTier.Should().Be(MembershipTier.Free);
        _gateway.Verify(g => g.InitializeAsync(It.IsAny<PaymentGatewayInitializeRequestDto>(), It.IsAny<CancellationToken>()), Times.Never);
        _subRepo.Verify(s => s.AddAsync(It.IsAny<UserSubscription>()), Times.Once);
    }

    [Fact]
    public async Task InitializePaymentAsync_WhenPaidPlan_ShouldInitializeGateway()
    {
        var plan = MakePlan();
        var user = MakeUser();
        _planRepo.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _gateway.Setup(g => g.InitializeAsync(It.IsAny<PaymentGatewayInitializeRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayInitializeResultDto { Succeeded = true, AuthorizationUrl = "https://paystack.test/authorize" });

        var result = await _sut.InitializePaymentAsync(plan.Id, user.Id, user.Email, "url");

        result.Succeeded.Should().BeTrue();
        result.RequiresRedirect.Should().BeTrue();
        result.AuthorizationUrl.Should().Be("https://paystack.test/authorize");
        _txRepo.Verify(t => t.AddAsync(It.Is<PaymentTransaction>(tx => tx.Status == PaymentStatus.Pending)), Times.Once);
    }

    [Fact]
    public async Task InitializePaymentAsync_WhenGatewayFails_ShouldReturnError()
    {
        var plan = MakePlan();
        var user = MakeUser();
        _planRepo.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _gateway.Setup(g => g.InitializeAsync(It.IsAny<PaymentGatewayInitializeRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayInitializeResultDto { Succeeded = false, ErrorMessage = "Gateway down" });

        var result = await _sut.InitializePaymentAsync(plan.Id, user.Id, user.Email, "url");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Gateway down");
    }

    // ── VerifyPaymentAsync ──

    [Fact]
    public async Task VerifyPaymentAsync_WhenEmptyReference_ShouldReturnError()
    {
        var result = await _sut.VerifyPaymentAsync("");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("reference");
    }

    [Fact]
    public async Task VerifyPaymentAsync_WhenGatewayFails_ShouldReturnError()
    {
        _gateway.Setup(g => g.VerifyAsync("ref-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayVerificationResultDto { Succeeded = false, ErrorMessage = "Verify failed" });

        var result = await _sut.VerifyPaymentAsync("ref-1");

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyPaymentAsync_WhenMetadataMissing_ShouldReturnError()
    {
        _gateway.Setup(g => g.VerifyAsync("ref-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayVerificationResultDto { Succeeded = true, Status = "success", Amount = 2500, RawResponse = "{}" });

        var result = await _sut.VerifyPaymentAsync("ref-1");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("verification failed");
    }

    [Fact]
    public async Task VerifyPaymentAsync_WhenAmountMismatch_ShouldReturnError()
    {
        var plan = MakePlan();
        var user = MakeUser();
        _gateway.Setup(g => g.VerifyAsync("ref-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayVerificationResultDto
            {
                Succeeded = true,
                Status = "success",
                Amount = 999,
                UserId = user.Id,
                PlanId = plan.Id
            });
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _planRepo.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);

        var result = await _sut.VerifyPaymentAsync("ref-1");

        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("amount");
    }

    [Fact]
    public async Task VerifyPaymentAsync_WhenValid_ShouldActivatePlanAndPersist()
    {
        var plan = MakePlan();
        var user = MakeUser();
        var userId = user.Id;
        var planId = plan.Id;
        _gateway.Setup(g => g.VerifyAsync("ref-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayVerificationResultDto
            {
                Succeeded = true,
                Status = "success",
                Reference = "ref-1",
                Amount = 2500,
                UserId = userId,
                PlanId = planId,
                RawResponse = "{\"status\":\"success\"}"
            });
        _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _planRepo.Setup(r => r.GetByIdAsync(planId)).ReturnsAsync(plan);
        _txRepo.Setup(r => r.GetByReferenceAsync("ref-1")).ReturnsAsync((PaymentTransaction?)null);
        _subRepo.Setup(r => r.GetActiveByUserAsync(userId)).ReturnsAsync((UserSubscription?)null);

        var result = await _sut.VerifyPaymentAsync("ref-1");

        result.Succeeded.Should().BeTrue();
        user.MembershipTier.Should().Be(MembershipTier.Premium);
        _txRepo.Verify(t => t.AddAsync(It.Is<PaymentTransaction>(tx =>
            tx.Status == PaymentStatus.Success && tx.Amount == 2500 && tx.Reference == "ref-1")), Times.Once);
        _subRepo.Verify(s => s.AddAsync(It.Is<UserSubscription>(sub =>
            sub.UserId == userId && sub.PlanId == planId && sub.IsActive)), Times.Once);
    }

    [Fact]
    public async Task VerifyPaymentAsync_WhenTransactionAlreadyPending_ShouldUpgradeToSuccess()
    {
        var plan = MakePlan();
        var user = MakeUser();
        var existing = new PaymentTransaction
        {
            UserId = user.Id,
            PlanId = plan.Id,
            Amount = 2500,
            Status = PaymentStatus.Pending,
            Reference = "ref-1"
        };
        _gateway.Setup(g => g.VerifyAsync("ref-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayVerificationResultDto
            {
                Succeeded = true,
                Status = "success",
                Reference = "ref-1",
                Amount = 2500,
                UserId = user.Id,
                PlanId = plan.Id,
                RawResponse = "{}"
            });
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _planRepo.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);
        _txRepo.Setup(r => r.GetByReferenceAsync("ref-1")).ReturnsAsync(existing);
        _subRepo.Setup(r => r.GetActiveByUserAsync(user.Id)).ReturnsAsync((UserSubscription?)null);

        var result = await _sut.VerifyPaymentAsync("ref-1");

        result.Succeeded.Should().BeTrue();
        existing.Status.Should().Be(PaymentStatus.Success);
        existing.PaidAtUtc.Should().NotBeNull();
    }

    // ── HandleWebhookAsync ──

    [Fact]
    public async Task HandleWebhookAsync_WhenChargeSuccess_ShouldVerifyReference()
    {
        var payload = "{\"event\":\"charge.success\",\"data\":{\"reference\":\"ref-9\"}}";
        _gateway.Setup(g => g.VerifyAsync("ref-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayVerificationResultDto { Succeeded = false, ErrorMessage = "nope" });

        await _sut.HandleWebhookAsync("charge.success", payload);

        _gateway.Verify(g => g.VerifyAsync("ref-9", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleWebhookAsync_WhenUnknownEvent_ShouldNotCallGateway()
    {
        await _sut.HandleWebhookAsync("invoice.update", "{}");

        _gateway.Verify(g => g.VerifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleWebhookAsync_WhenChargeSuccessWithoutReference_ShouldNotVerify()
    {
        await _sut.HandleWebhookAsync("charge.success", "{\"event\":\"charge.success\",\"data\":{}}");

        _gateway.Verify(g => g.VerifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyPaymentAsync_WhenTransactionAlreadySuccessful_ShouldShortCircuitWithoutCallingGateway()
    {
        var plan = MakePlan();
        var existingTx = new PaymentTransaction
        {
            Reference = "ref-already-done",
            Status = PaymentStatus.Success,
            PlanId = plan.Id
        };
        _txRepo.Setup(r => r.GetByReferenceAsync("ref-already-done")).ReturnsAsync(existingTx);
        _planRepo.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);

        var result = await _sut.VerifyPaymentAsync("ref-already-done");

        result.Succeeded.Should().BeTrue();
        result.PlanName.Should().Be(plan.Name);
        _gateway.Verify(g => g.VerifyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyPaymentAsync_WhenVerificationSucceeds_ShouldSendPaymentReceiptEmail()
    {
        var plan = MakePlan();
        var user = MakeUser();
        _gateway.Setup(g => g.VerifyAsync("ref-receipt-test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayVerificationResultDto
            {
                Succeeded = true,
                Status = "success",
                Reference = "ref-receipt-test",
                Amount = 2500,
                UserId = user.Id,
                PlanId = plan.Id,
                RawResponse = "{}"
            });
        _userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _planRepo.Setup(r => r.GetByIdAsync(plan.Id)).ReturnsAsync(plan);
        _txRepo.Setup(r => r.GetByReferenceAsync("ref-receipt-test")).ReturnsAsync((PaymentTransaction?)null);
        _subRepo.Setup(r => r.GetActiveByUserAsync(user.Id)).ReturnsAsync((UserSubscription?)null);

        var result = await _sut.VerifyPaymentAsync("ref-receipt-test");

        result.Succeeded.Should().BeTrue();
        _notifier.Verify(n => n.SendPaymentReceiptAsync(user.Email, plan.Name, plan.MonthlyPrice, "ref-receipt-test"), Times.Once);
    }
}
