using Microsoft.Extensions.Hosting;
namespace AIAgentMod;

public static class ModuleExtensions
{
    /// <summary>
    /// module services or init task
    /// </summary>
    public static IHostApplicationBuilder AddAIAgentMod(this IHostApplicationBuilder builder)
    {
        return builder;
    }
}