using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Application;
using MusicDistributionSystem.Domain.Constants;
using MusicDistributionSystem.Domain.Contracts.Security;
using MusicDistributionSystem.Infrastructure;
using MusicDistributionSystem.Infrastructure.Configuration;
using MusicDistributionSystem.Infrastructure.Data;
using MusicDistributionSystem.Infrastructure.EntityFrameworkCore;
using MusicDistributionSystem.Middleware;
using Serilog;
using Serilog.Events;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);

var logsDirectory = Path.Combine(builder.Environment.ContentRootPath, "Logs");
Directory.CreateDirectory(logsDirectory);

builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(logsDirectory, "app-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        shared: true));

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddMemoryCache();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(
    builder.Configuration.GetConnectionString("DefaultConnection")!,
    builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(RoleNames.SuperAdmin, RoleNames.Admin));
    options.AddPolicy("CanModerateContent", policy => policy.RequireRole(RoleNames.SuperAdmin, RoleNames.Admin, RoleNames.Moderator));
    options.AddPolicy("CanUploadContent", policy => policy.RequireRole(RoleNames.SuperAdmin, RoleNames.Admin, RoleNames.Artist));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var passwordHasherService = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
    var defaultAdminSettings = scope.ServiceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<DefaultAdminSettings>>()
        .Value;
    var shouldResetDatabaseOnStartup =
        app.Environment.IsDevelopment() &&
        builder.Configuration.GetValue<bool>("DatabaseSettings:ResetDatabaseOnStartup");

    if (shouldResetDatabaseOnStartup)
    {
        await context.Database.EnsureDeletedAsync();
    }

    var hasDefinedMigrations = context.Database.GetMigrations().Any();

    if (hasDefinedMigrations)
    {
        if (app.Environment.IsDevelopment() && await HasLegacySchemaWithoutMigrationHistoryAsync(context))
        {
            startupLogger.LogWarning(
                "Existing development database contains application tables but no EF migration history. Recreating the local database so migrations can be applied cleanly.");
            await context.Database.EnsureDeletedAsync();
        }

        await context.Database.MigrateAsync();
    }
    else
    {
        startupLogger.LogWarning(
            "No EF Core migrations were found in the application assembly. Falling back to EnsureCreated for local startup. Re-add migrations to restore the normal migration workflow.");
        await context.Database.EnsureCreatedAsync();
    }

    await DbInitializer.SeedAsync(context, passwordHasherService, defaultAdminSettings);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static async Task<bool> HasLegacySchemaWithoutMigrationHistoryAsync(ApplicationDbContext context)
{
    if (!await context.Database.CanConnectAsync())
    {
        return false;
    }

    var connection = context.Database.GetDbConnection();
    var shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;

    if (shouldCloseConnection)
    {
        await connection.OpenAsync();
    }

    try
    {
        var hasMigrationHistory = await ExecuteScalarIntAsync(
            connection,
            "SELECT COUNT(*) FROM sys.tables WHERE name = '__EFMigrationsHistory';");

        if (hasMigrationHistory > 0)
        {
            return false;
        }

        var applicationTableCount = await ExecuteScalarIntAsync(
            connection,
            @"SELECT COUNT(*) FROM sys.tables
              WHERE name IN ('Categories', 'MembershipPlans', 'Users', 'Roles', 'MusicTracks');");

        return applicationTableCount > 0;
    }
    finally
    {
        if (shouldCloseConnection)
        {
            await connection.CloseAsync();
        }
    }
}

static async Task<int> ExecuteScalarIntAsync(DbConnection connection, string commandText)
{
    await using var command = connection.CreateCommand();
    command.CommandText = commandText;
    var result = await command.ExecuteScalarAsync();
    return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
}

