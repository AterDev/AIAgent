using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MigrationService.DesignTime;

public class DesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        Console.WriteLine("DesignTimeServices.ConfigureDesignTimeServices invoked (MigrationService)");
        try
        {
            IServiceProvider tempProvider = null!;
            tempProvider = services.BuildServiceProvider(validateScopes: false);
            using var scope = tempProvider.CreateScope();
            IMigrationsModelDiffer? inner = null;

            inner = scope.ServiceProvider.GetService<IMigrationsModelDiffer>();
            services.AddSingleton<IMigrationsModelDiffer>(sp =>
            {
                var proxy = MigrationsModelDifferProxy.Create<IMigrationsModelDiffer>(inner!);
                return proxy;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("DesignTimeServices: exception while registering proxy: " + ex.Message);
        }
    }
}
