
using Entity.AIAgentMod;
using Entity.SystemMod;

namespace EntityFramework.AppDbContext;

/// <summary>
/// default data access for main business
/// </summary>
/// <param name="options"></param>
public partial class DefaultDbContext(DbContextOptions<DefaultDbContext> options)
    : ContextBase(options)
{
    #region SystemMod
    public DbSet<SystemUser> SystemUsers { get; set; }
    public DbSet<SystemRole> SystemRoles { get; set; }
    public DbSet<SystemUserRole> SystemUserRoles { get; set; }
    public DbSet<SystemConfig> SystemConfigs { get; set; }

    /// <summary>
    /// 菜单
    /// </summary>
    public DbSet<SystemMenu> SystemMenus { get; set; }
    public DbSet<SystemPermission> SystemPermissions { get; set; }

    /// <summary>
    /// 权限组
    /// </summary>
    public DbSet<SystemPermissionGroup> SystemPermissionGroups { get; set; }
    public DbSet<SystemLogs> SystemLogs { get; set; }
    public DbSet<SystemOrganization> SystemOrganizations { get; set; } = null!;
    #endregion

    #region AIAgentMod

    public DbSet<AIAgent> AIAgents { get; set; }

    public DbSet<TokenUsage> TokenUsages { get; set; }

    public DbSet<AIModelInfo> AIModelInfos { get; set; }
    public DbSet<AIModelProvider> AIModelProviders { get; set; }

    public DbSet<ChatMessage> ChatMessages { get; set; }

    public DbSet<Conversation> Conversations { get; set; }

    public DbSet<MCPServerInfo> MCPServerInfos { get; set; }

    public DbSet<TokenUsageRecord> TokenUsageRecords { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<SystemLogs>().Ignore(e => e.IsDeleted);

        builder
            .Entity<SystemUser>()
            .HasMany(u => u.SystemRoles)
            .WithMany(r => r.Users)
            .UsingEntity<SystemUserRole>(
                j => j.HasOne(ur => ur.Role).WithMany().HasForeignKey(ur => ur.RoleId),
                j => j.HasOne(ur => ur.User).WithMany().HasForeignKey(ur => ur.UserId),
                j =>
                {
                    j.HasKey(ur => ur.Id);
                }
            );
        builder
            .Entity<SystemMenu>()
            .HasMany(u => u.SystemRoles)
            .WithMany(r => r.SystemMenus)
            .UsingEntity<SystemMenuRole>(
                j => j.HasOne(ur => ur.SystemRole).WithMany().HasForeignKey(ur => ur.RoleId),
                j => j.HasOne(ur => ur.SystemMenu).WithMany().HasForeignKey(ur => ur.MenuId),
                j =>
                {
                    j.HasKey(ur => ur.Id);
                }
            );
    }
}
