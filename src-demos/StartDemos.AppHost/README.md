
# Demos说明


## 自建项目注意点

### 1.配置appsettings.json
- Components
- Authentication
- Cache
- Otel
- Cors


### 2.注册服务
```
# Program.cs

// 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
builder.AddServiceDefaults();

// 框架依赖服务:options, cache, dbContext
builder.AddFrameworkServices();

// Web中间件服务:route, openapi, jwt, cors, auth, rateLimiter etc.
builder.AddMiddlewareServices();
```

### 3.使用Managers和Models服务
不通过templates建立项目，而是通过VS自己创建服务项目时，为了使用服务：
```
# Program.cs
using XXXX.Extension;

// 业务Managers
builder.Services.AddManagers();

// 模块服务
builder.AddModules();
 ```

 则需要：
- 添加代码生成项目（Perigon.AspNetCore.SourceGeneration.csproj），并设置OutputItemType,ReferenceOutputAssembly：
```
# AdminDemoApp.csproj
# WebDemoApp.csproj
<ProjectReference Include="Perigon.AspNetCore.SourceGeneration.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```


### 4.在AppHost中使用类库方法
- AspreHost中引用类库时，需要设置：IsAspireProjectResource，方便使用类库的方法和扩展。
```
# AspireHost.csproj
<ProjectReference Include="Demos.Core.csproj" IsAspireProjectResource="false" />
```
