using AspNetExample.Database.Context.Factory;

namespace AspNetExample;

public static class AspNetExampleModule
{
    public static void RegisterDependencies(IServiceCollection service)
    {
        service.AddSingleton<IApplicationContextFactory, ApplicationContextFactory>();
    }
}