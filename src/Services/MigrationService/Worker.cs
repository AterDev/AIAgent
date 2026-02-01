using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger
) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity(
            "Migrating database",
            ActivityKind.Client
        );
        try
        {
            using var scope = serviceProvider.CreateScope();
            _logger.LogInformation("migrations {db}", nameof(DefaultDbContext));
            var dbContext = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }

    private static async Task RunMigrationAsync<T>(T dbContext, CancellationToken cancellationToken)
        where T : DbContext
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync<T>(T dbContext, CancellationToken cancellationToken)
        where T : DbContext
    {
        if (dbContext is not DefaultDbContext defaultDb)
        {
            return;
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // 检查是否已有用户
            if (await defaultDb.SystemUsers.AnyAsync(cancellationToken))
            {
                return;
            }

            // 添加默认管理员用户
            var passwordSalt = HashCrypto.BuildSalt();
            var adminUser = new SystemUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = "admin@default.com",
                RealName = "System Administrator",
                Phone = "13800138000",
                PasswordSalt = passwordSalt,
                PasswordHash = HashCrypto.GeneratePwd("Perigon.2026", passwordSalt),
                Roles = WebConst.SuperAdmin,
                Enabled = true
            };

            defaultDb.SystemUsers.Add(adminUser);
            await defaultDb.SaveChangesAsync(cancellationToken);
        });
    }
}
