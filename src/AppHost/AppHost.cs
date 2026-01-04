using AppHost;
using Perigon.AspNetCore.Constants;

var builder = DistributedApplication.CreateBuilder(args);
var aspireSetting = AppSettingsHelper.LoadAspireSettings(builder.Configuration);

IResourceBuilder<IResourceWithConnectionString>? database = null;
IResourceBuilder<IResourceWithConnectionString>? cache = null;

// if you have exist resource, you can set connection string here, without create container
// database = builder.AddConnectionString(AppConst.Default, "");
// nats = builder.AddConnectionString("mq", "");
// qdrant = builder.AddConnectionString("qdrant", "");

#region infrastructure
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

#endregion

#region services
var serviceGroup = builder.AddGroup("Services", "Globe");
var migration = builder.AddProject<Projects.MigrationService>("MigrationService")
    .WithParentRelationship(serviceGroup);
var apiService = builder.AddProject<Projects.ApiService>("ApiService").WaitForCompletion(migration)
    .WithParentRelationship(serviceGroup);
var adminService = builder.AddProject<Projects.AdminService>("AdminService").WaitForCompletion(migration)
    .WithParentRelationship(serviceGroup);

// run frontend app, you should install npm packages first
var webApp = builder.AddJavaScriptApp("frontend", "../ClientApp/WebApp", "start")
    .WithPnpm()
    .WithUrl("http://localhost:4200")
    .WaitFor(adminService)
    .WithParentRelationship(serviceGroup);

if (database != null)
{
    migration.WithReference(database).WaitFor(database);
    apiService.WithReference(database);
    adminService.WithReference(database);
}
if (cache != null)
{
    migration.WithReference(cache).WaitFor(cache);
    apiService.WithReference(cache);
    adminService.WithReference(cache);
}
# endregion

builder.Build().Run();
