using Entity.AIAgentMod;
using Entity.CoreMod;
using Entity.KnowledgeBaseMod;
using Entity.McpMod;
using Entity.ModelMod;
using Entity.SystemMod;
using Entity.WorkflowMod;

namespace EntityFramework.AppDbContext;

/// <summary>
/// default data access for main business
/// </summary>
/// <param name="options"></param>
public partial class DefaultDbContext(DbContextOptions<DefaultDbContext> options)
    : ContextBase(options)
{
    #region AIAgentMod

    public DbSet<AIAgent> AIAgents { get; set; }

    public DbSet<ApplicationAgent> ApplicationAgents { get; set; }

    public DbSet<TokenUsage> TokenUsages { get; set; }

    public DbSet<ChatMessage> ChatMessages { get; set; }

    public DbSet<Conversation> Conversations { get; set; }

    public DbSet<MCPServerInfo> MCPServerInfos { get; set; }

    public DbSet<TokenUsageRecord> TokenUsageRecords { get; set; }

    public DbSet<AgentExecution> AgentExecutions { get; set; }

    public DbSet<A2ARemoteAgent> A2ARemoteAgents { get; set; }

    #endregion

    #region ModelMod

    public DbSet<Application> Applications { get; set; }

    public DbSet<ApiKeyAuthIndex> ApiKeyAuthIndexes { get; set; }

    public DbSet<ApplicationQuota> ApplicationQuotas { get; set; }

    public DbSet<QuotaUsage> QuotaUsages { get; set; }

    public DbSet<ApplicationModelPermission> ApplicationModelPermissions { get; set; }

    public DbSet<ApplicationToolPermission> ApplicationToolPermissions { get; set; }

    public DbSet<ApplicationRagCollectionPermission> ApplicationRagCollectionPermissions { get; set; }

    public DbSet<AIModelProvider> AIModelProviders { get; set; }

    public DbSet<AIModelInfo> AIModelInfos { get; set; }

    public DbSet<ModelInvocation> ModelInvocations { get; set; }

    #endregion

    #region CoreMod

    public DbSet<AIPrompt> AIPrompts { get; set; }

    #endregion

    #region KnowledgeBaseMod

    public DbSet<RagCollection> RagCollections { get; set; }

    public DbSet<RagDocument> RagDocuments { get; set; }

    public DbSet<RagChunk> RagChunks { get; set; }

    public DbSet<DocumentParsingResult> DocumentParsingResults { get; set; }

    public DbSet<RagAgentConfig> RagAgentConfigs { get; set; }

    #endregion

    #region McpMod

    public DbSet<McpTool> McpTools { get; set; }

    public DbSet<ToolCallRecord> ToolCallRecords { get; set; }

    #endregion

    #region WorkflowMod

    public DbSet<Workflow> Workflows { get; set; }

    public DbSet<WorkflowExecution> WorkflowExecutions { get; set; }

    #endregion

    #region SystemMod

    public DbSet<SystemConfig> SystemConfigs { get; set; }

    public DbSet<SystemUser> SystemUsers { get; set; }

    public DbSet<StorageProvider> StorageProviders { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

    }
}
