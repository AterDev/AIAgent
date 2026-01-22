
//builder.AddCSharpApp("app", "./hello/app.cs")
//       .WithHttpEndpoint();

//builder.AddContainer("myapp", "mcr.microsoft.com/dotnet/samples", "aspnetapp")
//    .WithHttpEndpoint(name: "local", targetPort: 8080, port: 8080)
//    // This is the new extension method we'll implement
//    .WithReverseProxyEndpoint(name: "custom", url: "https://myapp.localhost:8080");

// python
//var app = builder.AddExecutable("my-app", "python", "app.py", ".")
//    .WithReference(redis)
//    .WithEnvironment(context =>
//    {
//        context.EnvironmentVariables["REDIS_HOST"] = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Host);
//        context.EnvironmentVariables["REDIS_PORT"] = redis.Resource.PrimaryEndpoint.Property(EndpointProperty.Port);
//    });