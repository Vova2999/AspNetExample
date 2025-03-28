using AspNetExample.Database.Context.Factory;
using AspNetExample.Database.Helpers;

namespace AspNetExample;

public static class AspNetExampleModule
{
    public static void RegisterDependencies(IServiceCollection service, ConfigurationManager configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        service.AddSingleton<IApplicationContextFactory, ApplicationContextFactory>();
        service.AddSingleton(_ => ApplicationContextHelper.BuildOptions(connectionString));
    }
}