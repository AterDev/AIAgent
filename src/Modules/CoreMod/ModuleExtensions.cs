using System.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace CoreMod;

[DisplayName("Perigon::CoreMod")]
[Description("Core module for technical invocation capabilities")]
public static class ModuleExtensions
{
    /// <summary>
    /// Module services or init task.
    /// </summary>
    public static IHostApplicationBuilder AddCoreMod(this IHostApplicationBuilder builder)
    {
        builder.AddModServices();
        return builder;
    }

    // The module services registration
    private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
    {
        // custom services registration
        return builder;
    }

    // The module middlewares registration
    public static WebApplication UseCoreModServices(this WebApplication app)
    {
       // custom middlewares and init task
       return app;
    }
}