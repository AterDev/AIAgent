using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Demos.Core;
using Microsoft.Extensions.DependencyInjection;
using Perigon.AspNetCore.Constants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection;

public static class DemoExtensions
{
    /*
      "postgresql" => database = builder
        .AddPostgres(name: "Database", password: devPassword, port: aspireSetting.DbPort)
        .WithImageTag("18.1-alpine")
        .WithDataVolume()
        .AddDatabase(AppConst.Default, databaseName: defaultName),
    "sqlserver" => database = builder
        .AddSqlServer(name: "Database", password: devPassword, port: aspireSetting.DbPort)
        .WithImageTag("2025-latest")
        .WithDataVolume()
        .AddDatabase(AppConst.Default, databaseName: defaultName),
     */
    public static void AddDemoDatabase(this IServiceCollection service)
    {
        //{ "title":"error","detail":"ConnectionString is missing. " +
        //        "It should be provided in 'ConnectionStrings:Default' " +
        //        "or under the 'ConnectionString' key in 'Aspire:Npgsql:EntityFrameworkCore:PostgreSQL' " +
        //        "or 'Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:DefaultDbContext' configuration section.","status":500,"traceId":"0HNIPM7KIDU9G:00000002"}

        //string db2 = "HOST=47.116.71.231;PORT=54321;DATABASE=dev_adm2025;USER ID=postgres;PASSWORD=xxxxx";
        // new ConnectionConfig() { ConfigId = DevConfigs.DB_POSTGRES, ConnectionString = db2, IsAutoCloseConnection = true, DbType = DbType.PostgreSQL }

    }


    public static void AddAspireDevDockerDatabase(this IDistributedApplicationBuilder builder, 
        AspireSetting aspireSetting,
        ref IResourceBuilder<IResourceWithConnectionString>? database,
        ref IResourceBuilder<IResourceWithConnectionString>? cache
      )
    {
        var defaultName = "AIAgent_dev";
        var devPassword = builder.AddParameter(
            "dev-password",
            value: aspireSetting.DevPassword,
            secret: true
        );

        var infrastructureGroup = builder.AddGroup("Infrastructure", "Cloud");
        _ = aspireSetting.DatabaseType?.ToLowerInvariant() switch
        {
            "postgresql" => database = builder
                .AddPostgres(name: "Database", password: devPassword, port: aspireSetting.DbPort)
                .WithImageTag("18.1-alpine")
                .WithDataVolume()
                .AddDatabase(AppConst.Default, databaseName: defaultName),
            "sqlserver" => database = builder
                .AddSqlServer(name: "Database", password: devPassword, port: aspireSetting.DbPort)
                .WithImageTag("2025-latest")
                .WithDataVolume()
                .AddDatabase(AppConst.Default, databaseName: defaultName),
            _ => null,

        };
        _ = aspireSetting.CacheType?.ToLowerInvariant() switch
        {
            "memory" => null,
            _ => cache = builder
                .AddRedis("Cache", password: devPassword, port: aspireSetting.CachePort)
                .WithImageTag("8.2-alpine")
                .WithDataVolume()
                .WithPersistence(interval: TimeSpan.FromMinutes(5)),
        };

        devPassword.WithParentRelationship(infrastructureGroup);
        database?.WithParentRelationship(infrastructureGroup);
        cache?.WithParentRelationship(infrastructureGroup);
    }


}
