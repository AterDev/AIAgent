using Microsoft.Extensions.Hosting;
using Perigon.AspNetCore.Constants;
using SystemMod.Models.SystemUserDtos;

namespace SystemMod.Services;

/// <summary>
/// module init host service
/// </summary>
public class InitSystemModService(
    IServiceProvider serviceProvider,
    ILogger<InitSystemModService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();

        try
        {
            logger.LogInformation("SystemMod initializing...");

            // 初始化管理员账号
            await InitializeAdminUserAsync(scope.ServiceProvider, stoppingToken);

            logger.LogInformation("SystemMod initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SystemMod initialization failed");
            return;
        }
    }

    /// <summary>
    /// 初始化管理员账号
    /// </summary>
    private async Task InitializeAdminUserAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        try
        {
            var manager = services.GetRequiredService<SystemUserManager>();

            // 检查管理员账号是否已存在
            if (await manager.ExistsUserNameAsync("admin"))
            {
                logger.LogInformation("Admin user already exists, skipping initialization");
                return;
            }

            // 创建管理员账号
            var adminDto = new SystemUserAddDto
            {
                UserName = "admin",
                Email = "admin@aiagent.local",
                RealName = "系统管理员",
                Password = "Perigon.2026",
                Roles = WebConst.SuperAdmin,
                Enabled = true
            };

            var adminUser = await manager.AddAsync(adminDto);
            logger.LogInformation("Admin user created successfully with username: {UserName}", adminUser.UserName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize admin user");
            throw;
        }
    }
}