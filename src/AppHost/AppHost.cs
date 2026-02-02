using AppHost;
using Perigon.AspNetCore.Constants;

var builder = DistributedApplication.CreateBuilder(args);
var aspireSetting = AppSettingsHelper.LoadAspireSettings(builder.Configuration);

var isTesting = builder.Configuration["ASPIRE_ENVIRONMENT"]?.ToLowerInvariant() == "testing";

IResourceBuilder<IResourceWithConnectionString>? database = null;
IResourceBuilder<IResourceWithConnectionString>? cache = null;
IResourceBuilder<QdrantServerResource>? qdrant = null;

// if you have exist resource, you can set connection string here, without create container
// database = builder.AddConnectionString(AppConst.Default, "");
// nats = builder.AddConnectionString("mq", "");
// qdrant = builder.AddQdrant("qdrant");

#region infrastructure
var defaultName = isTesting ? "AIAgent_test" : "AIAgent_dev";
var devPassword = builder.AddParameter(
    "dev-password",
    value: aspireSetting.DevPassword,
    secret: true
);

var infrastructureGroup = builder.AddGroup("Infrastructure", "Cloud");

// Add NATS message queue with JetStream support
var nats = builder.AddNats("nats", port: aspireSetting.NatsPort ?? 4222)
    .WithJetStream()  // Enable JetStream for distributed streams and consumers
    .WithParentRelationship(infrastructureGroup);
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

if (aspireSetting.VectorStoreType?.ToLowerInvariant() == "qdrant")
{
    qdrant = builder
        .AddQdrant("qdrant", apiKey: devPassword, httpPort: aspireSetting.QdrantPort)
        .WithDataVolume();
}

devPassword.WithParentRelationship(infrastructureGroup);
database?.WithParentRelationship(infrastructureGroup);
cache?.WithParentRelationship(infrastructureGroup);
qdrant?.WithParentRelationship(infrastructureGroup);

#endregion

#region services
var serviceGroup = builder.AddGroup("Services", "Globe");
var migration = builder.AddProject<Projects.MigrationService>("MigrationService")
    .WithParentRelationship(serviceGroup);
var apiService = builder.AddProject<Projects.ApiService>("ApiService").WaitForCompletion(migration)
    .WithEnvironment("OTEL_INSTRUMENTATION_GENAI_CAPTURE_MESSAGE_CONTENT", "true")
    .WithParentRelationship(serviceGroup);
var adminService = builder.AddProject<Projects.AdminService>("AdminService").WaitForCompletion(migration)
    .WithEnvironment("OTEL_INSTRUMENTATION_GENAI_CAPTURE_MESSAGE_CONTENT", "true")
    .WithParentRelationship(serviceGroup);
var fileProcessor = builder.AddProject<Projects.FileProcessorService>("FileProcessorService").WaitForCompletion(migration)
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
    fileProcessor.WithReference(database);
}
if (cache != null)
{
    migration.WithReference(cache).WaitFor(cache);
    apiService.WithReference(cache);
    adminService.WithReference(cache);
    fileProcessor.WithReference(cache);
}
if (qdrant != null)
{
    apiService.WithReference(qdrant);
    adminService.WithReference(qdrant);
    fileProcessor.WithReference(qdrant);
}

adminService.WithReference(nats);
fileProcessor.WithReference(nats).WaitFor(nats);
# endregion

builder.Build().Run();
