using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using MusicDistributionSystem.Application.Contracts.Payments;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Payment;
using MusicDistributionSystem.Application.Services;
using MusicDistributionSystem.Domain.Contracts.Logging;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;
using MusicDistributionSystem.Infrastructure.Configuration;
using MusicDistributionSystem.Infrastructure.EntityFrameworkCore;
using MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories;
using MusicDistributionSystem.Infrastructure.Notifications;
using MusicDistributionSystem.Infrastructure.Services;
using System.Net.Http;

namespace MusicDistributionSystem.Tests.IntegrationTests;

public class LiveIntegrationTests
{
    private readonly IConfiguration _configuration;
    private readonly PaystackSettings _paystackSettings;
    private readonly EmailSettings _emailSettings;
    private readonly Mock<IAppLogger> _loggerMock;

    public LiveIntegrationTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddUserSecrets("MusicDistributionSystem-Web-12345678")
            .Build();

        _paystackSettings = new PaystackSettings
        {
            PublicKey = _configuration["Paystack:PublicKey"] ?? string.Empty,
            SecretKey = _configuration["Paystack:SecretKey"] ?? string.Empty,
            CallbackUrl = _configuration["Paystack:CallbackUrl"] ?? "https://localhost:5001/Payment/Callback"
        };

        _emailSettings = new EmailSettings
        {
            SmtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com",
            Port = int.TryParse(_configuration["EmailSettings:Port"], out var port) ? port : 587,
            EnableSsl = !bool.TryParse(_configuration["EmailSettings:EnableSsl"], out var ssl) || ssl,
            SenderName = _configuration["EmailSettings:SenderName"] ?? "SoundSphere Support",
            SenderEmail = _configuration["EmailSettings:SenderEmail"] ?? string.Empty,
            Username = _configuration["EmailSettings:Username"] ?? string.Empty,
            Password = _configuration["EmailSettings:Password"] ?? string.Empty
        };

        _loggerMock = new Mock<IAppLogger>();
    }

    [Fact]
    public void Configuration_ShouldLoadPaystackAndEmailSettings()
    {
        _paystackSettings.SecretKey.Should().NotBeNullOrWhiteSpace();
        _paystackSettings.PublicKey.Should().NotBeNullOrWhiteSpace();
        _paystackSettings.SecretKey.Should().StartWith("sk_test_");
        _paystackSettings.PublicKey.Should().StartWith("pk_test_");

        _emailSettings.SmtpServer.Should().Be("smtp.gmail.com");
        _emailSettings.Port.Should().Be(587);
        _emailSettings.SenderEmail.Should().NotBeNullOrWhiteSpace();
        _emailSettings.Username.Should().NotBeNullOrWhiteSpace();
        _emailSettings.Password.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Paystack_LiveApi_InitializePayment_ShouldReturnValidAuthorizationUrl()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("Paystack", client =>
        {
            client.BaseAddress = new Uri("https://api.paystack.co");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IHttpClientFactory>();

        var gateway = new PaystackGateway(factory, Options.Create(_paystackSettings), _loggerMock.Object);

        var testReference = $"MDS-TEST-{Guid.NewGuid():N}";
        var request = new PaymentGatewayInitializeRequestDto
        {
            Email = _emailSettings.SenderEmail,
            Amount = 2500, // 2,500 NGN
            Reference = testReference,
            CallbackUrl = _paystackSettings.CallbackUrl,
            UserId = Guid.NewGuid(),
            PlanId = Guid.NewGuid()
        };

        var result = await gateway.InitializeAsync(request);

        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue($"Paystack initialization failed: {result.ErrorMessage}");
        result.AuthorizationUrl.Should().NotBeNullOrWhiteSpace();
        result.AuthorizationUrl.Should().StartWith("https://checkout.paystack.com/");
        result.Reference.Should().Be(testReference);
    }

    [Fact]
    public async Task Paystack_LiveApi_VerifyInvalidReference_ShouldFailGracefully()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("Paystack", client =>
        {
            client.BaseAddress = new Uri("https://api.paystack.co");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IHttpClientFactory>();

        var gateway = new PaystackGateway(factory, Options.Create(_paystackSettings), _loggerMock.Object);

        var result = await gateway.VerifyAsync("NON_EXISTENT_REF_999999");

        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task EmailService_LiveSmtp_SendVerificationEmail_ShouldSucceed()
    {
        var emailService = new AccountNotificationService(Options.Create(_emailSettings), _loggerMock.Object);
        var testCode = "789123";

        var act = async () => await emailService.SendVerificationCodeAsync(_emailSettings.SenderEmail, testCode);

        await act.Should().NotThrowAsync();
        _loggerMock.Verify(l => l.LogInformationAsync("AccountNotification", It.Is<string>(s => s.Contains("Email sent to"))), Times.Once);
    }

    [Fact]
    public async Task EmailService_LiveSmtp_SendPasswordResetEmail_ShouldSucceed()
    {
        var emailService = new AccountNotificationService(Options.Create(_emailSettings), _loggerMock.Object);
        var testCode = "456789";

        var act = async () => await emailService.SendPasswordResetCodeAsync(_emailSettings.SenderEmail, testCode);

        await act.Should().NotThrowAsync();
        _loggerMock.Verify(l => l.LogInformationAsync("AccountNotification", It.Is<string>(s => s.Contains("Email sent to"))), Times.Once);
    }

    [Fact]
    public async Task EmailService_LiveSmtp_SendPaymentReceiptEmail_ShouldSucceed()
    {
        var emailService = new AccountNotificationService(Options.Create(_emailSettings), _loggerMock.Object);

        var act = async () => await emailService.SendPaymentReceiptAsync(_emailSettings.SenderEmail, "Premium Pulse (Live Test)", 2500, "MDS-LIVE-TEST-001");

        await act.Should().NotThrowAsync();
        _loggerMock.Verify(l => l.LogInformationAsync("AccountNotification", It.Is<string>(s => s.Contains("Email sent to"))), Times.Once);
    }

    [Fact]
    public async Task CompleteWorkflow_InitializeWithLivePaystack_AndVerifyPayment_ShouldUpdateDbAndSendReceipt()
    {
        // 1. Setup in-memory DbContext
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"LiveWorkflowDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ApplicationDbContext(options);

        var user = new User
        {
            Username = "livetestuser",
            Email = _emailSettings.SenderEmail,
            PasswordHash = "hashed_pw",
            IsActive = true,
            IsEmailVerified = true,
            MembershipTier = MembershipTier.Free
        };

        var plan = new MembershipPlan
        {
            Name = "Pro Producer Live",
            Tier = MembershipTier.Premium,
            MonthlyPrice = 5000,
            MonthlyDownloadLimit = 500
        };

        context.Users.Add(user);
        context.MembershipPlans.Add(plan);
        await context.SaveChangesAsync();

        var planRepo = new MembershipPlanRepository(context);
        var userRepo = new UserRepository(context);
        var txRepo = new PaymentTransactionRepository(context);
        var subRepo = new UserSubscriptionRepository(context);

        // 2. Setup real PaystackGateway
        var services = new ServiceCollection();
        services.AddHttpClient("Paystack", client =>
        {
            client.BaseAddress = new Uri("https://api.paystack.co");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IHttpClientFactory>();

        var gateway = new PaystackGateway(factory, Options.Create(_paystackSettings), _loggerMock.Object);
        var emailService = new AccountNotificationService(Options.Create(_emailSettings), _loggerMock.Object);

        var paymentService = new PaymentService(
            planRepo,
            userRepo,
            txRepo,
            subRepo,
            gateway,
            emailService,
            _loggerMock.Object);

        // 3. Initialize payment with live Paystack API
        var initResult = await paymentService.InitializePaymentAsync(plan.Id, user.Id, user.Email, _paystackSettings.CallbackUrl);

        initResult.Succeeded.Should().BeTrue();
        initResult.RequiresRedirect.Should().BeTrue();
        initResult.AuthorizationUrl.Should().StartWith("https://checkout.paystack.com/");

        // 4. Verify database has Pending transaction with generated reference
        var pendingTx = await context.PaymentTransactions.FirstOrDefaultAsync(t => t.UserId == user.Id && t.PlanId == plan.Id);
        pendingTx.Should().NotBeNull();
        pendingTx!.Status.Should().Be(PaymentStatus.Pending);
        pendingTx.Amount.Should().Be(5000);
        pendingTx.Reference.Should().StartWith("MDS-");

        // 5. Test Verification flow with gateway mock or verify callback
        var mockGatewayForVerify = new Mock<IPaymentGateway>();
        mockGatewayForVerify.Setup(g => g.VerifyAsync(pendingTx.Reference, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGatewayVerificationResultDto
            {
                Succeeded = true,
                Status = "success",
                Reference = pendingTx.Reference,
                Amount = 5000,
                UserId = user.Id,
                PlanId = plan.Id,
                RawResponse = "{\"status\":true,\"data\":{\"status\":\"success\"}}"
            });

        var paymentServiceWithVerify = new PaymentService(
            planRepo,
            userRepo,
            txRepo,
            subRepo,
            mockGatewayForVerify.Object,
            emailService,
            _loggerMock.Object);

        var verifyResult = await paymentServiceWithVerify.VerifyPaymentAsync(pendingTx.Reference);

        verifyResult.Succeeded.Should().BeTrue();
        verifyResult.PlanName.Should().Be("Pro Producer Live");

        // 6. Verify Database State
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser!.MembershipTier.Should().Be(MembershipTier.Premium);

        var updatedTx = await context.PaymentTransactions.FirstOrDefaultAsync(t => t.Reference == pendingTx.Reference);
        updatedTx!.Status.Should().Be(PaymentStatus.Success);
        updatedTx.PaidAtUtc.Should().NotBeNull();

        var subscription = await context.UserSubscriptions.FirstOrDefaultAsync(s => s.UserId == user.Id && s.PlanId == plan.Id);
        subscription.Should().NotBeNull();
        subscription!.IsActive.Should().BeTrue();
        subscription.EndDateUtc.Should().BeAfter(DateTime.UtcNow);

        // 7. Verify Idempotency: Second verification should succeed immediately without duplicate processing
        var secondVerifyResult = await paymentServiceWithVerify.VerifyPaymentAsync(pendingTx.Reference);
        secondVerifyResult.Succeeded.Should().BeTrue();

        var subCount = await context.UserSubscriptions.CountAsync(s => s.UserId == user.Id && s.PlanId == plan.Id && s.IsActive);
        subCount.Should().Be(1);
    }
}
