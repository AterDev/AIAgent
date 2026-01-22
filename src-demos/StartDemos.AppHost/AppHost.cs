using Aspire.Hosting;
using Aspire.Hosting.JavaScript;
using Demos.Core;
using Microsoft.Extensions.DependencyInjection;

// 注意引用[ Demos.Core]：IsAspireProjectResource="false"

// builder
var builder = DistributedApplication.CreateBuilder(args);
var aspireSetting = Demos.Core.AppSettingsHelper.LoadAspireSettings(builder.Configuration);

// groups
var adminGroup = builder.AddGroup("AdminWebApp", "Globe");
var portalGroup = builder.AddGroup("PortalWebApp", "Globe");


// start database


#region infrastructure

IResourceBuilder<IResourceWithConnectionString>? database = null;
IResourceBuilder<IResourceWithConnectionString>? cache = null;

// 添加开发数据库
builder.AddAspireDevDockerDatabase(aspireSetting, ref database, ref cache);

#endregion

// start admin webs | 后台系统
var adminService = builder.AddProject<Projects.AdminDemoApp>("AdminDemos")
    .WithParentRelationship(adminGroup)
    .WithUrl("http://localhost:5010/health");

// start portabl webs | 门户系统
var portalService = builder.AddProject<Projects.WebDemoApp>("PortalDemos")
    .WithParentRelationship(portalGroup)
    .WithUrl("http://localhost:5050/health");

// "dev": "npm run start:dev",
var pnpmApp = builder.AddJavaScriptApp("WebApp-pro", "../../src/ClientApp/WebApp-pro")
    .WithPnpm(install: true)
    .WithRunScript("dev")
    .WithUrl("http://localhost:8000", "通用后台antd-pro")
    .WithParentRelationship(adminGroup)
    ;

if (database != null)
{
    adminService.WithReference(database);
    portalService.WithReference(database);
}
if (cache != null)
{
    portalService.WithReference(cache);
    adminService.WithReference(cache);
}


builder.Build().Run();
