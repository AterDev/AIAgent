// Here you could define global logic that would affect all tests

// You can use attributes at the assembly level to apply to all tests in the assembly

using Aspire.Hosting;
using Microsoft.Extensions.Logging;
using Perigon.AspNetCore.Constants;

[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace ApiTest;

public class GlobalHooks
{
    public static DistributedApplication? App { get; private set; }
    public static ResourceNotificationService? NotificationService { get; private set; }

    [Before(TestSession)]
    public static async Task SetUp()
    {

        Environment.SetEnvironmentVariable("ASPIRE_ENVIRONMENT", "Testing");
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        appHost.Services.AddLogging(logging => logging
            .AddConsole()
            .AddFilter("Default", LogLevel.Information)
            .AddFilter("Microsoft.AspNetCore", LogLevel.Warning)
            .AddFilter("Aspire.Hosting.Dcp", LogLevel.Warning));

        App = await appHost.BuildAsync();
        NotificationService = App.Services.GetRequiredService<ResourceNotificationService>();
        await App.StartAsync();
    }

    [After(TestSession)]
    public static async Task CleanUp()
    {
        if (App != null)
        {
            var connectionString = await App.GetConnectionStringAsync(AppConst.Default);

            var defaultConnection = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = defaultConnection.Database;
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString)
            {
                Database = "postgres"
            };
            using var conn = new Npgsql.NpgsqlConnection(builder.ToString());
            await conn.OpenAsync();

            // 强制断开所有连接并删除库
            var dropSql = $"DROP DATABASE IF EXISTS \"{databaseName}\" WITH (FORCE);";
            using var cmd = new Npgsql.NpgsqlCommand(dropSql, conn);
            await cmd.ExecuteNonQueryAsync();

            await App.StopAsync();
            await App.DisposeAsync();
        }
        Console.WriteLine("...and after!");
    }
}
