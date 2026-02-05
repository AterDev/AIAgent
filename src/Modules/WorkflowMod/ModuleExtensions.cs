using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using WorkflowMod.Services;

namespace WorkflowMod;

[DisplayName("Perigon::WorkflowMod")]
[Description("Workflow definition and execution module")]
public static class ModuleExtensions
{
    /// <summary>
    /// Module services or init task.
    /// </summary>
    public static IHostApplicationBuilder AddWorkflowMod(this IHostApplicationBuilder builder)
    {
        builder.AddModServices();
        return builder;
    }

    // The module services registration
    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IEntityTaskQueue<WorkflowTask>>(new EntityTaskQueue<WorkflowTask>());
        builder.Services.AddSingleton<WorkflowQueue>();
        builder.Services.AddScoped<WorkflowExecutor>();
        builder.Services.AddHostedService<WorkflowWorker>();
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseWorkflowModServices(this WebApplication app)
    {
        // custom middlewares and init task
        return app;
    }
}