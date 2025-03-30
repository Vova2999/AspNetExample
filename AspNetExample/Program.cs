using System.ComponentModel;
using AspNetExample.Common.Extensions;
using AspNetExample.Converters;
using AspNetExample.Domain.Entities;
using AspNetExample.Helpers;
using AspNetExample.Middlewares;
using AspNetExample.NSwag;
using AspNetExample.Services.Startup;
using AspNetExample.Services.Stores;
using Microsoft.AspNetCore.Identity;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;

namespace AspNetExample;

public static class Program
{
    public static void Main(string[] args)
    {
        ComponentModelResourceManagerHelper.OverrideResourceManager();

        var builder = WebApplication.CreateBuilder(args);

        LogManager.Configuration = new NLogLoggingConfiguration(
            builder.Configuration.GetSection("NLog"));

        builder.Services.AddIdentity<User, Role>()
            .AddUserStore<ApplicationContextUserStore>()
            .AddRoleStore<ApplicationContextRoleStore>();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 5;
            options.Password.RequiredUniqueChars = 1;
        });

        builder.Services
            .AddControllersWithViews(options =>
            {
                options.AllowEmptyInputInBodyModelBinding = true;
                ComponentModelResourceManagerHelper.SetAccessorMessages(
                    options.ModelBindingMessageProvider);
            })
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new DateOnlyCurrentCultureJsonConverter()))
            .ConfigureApiBehaviorOptions(options =>
                options.SuppressModelStateInvalidFilter = true);

        TypeDescriptor.AddAttributes(typeof(DateOnly), new TypeConverterAttribute(typeof(DateOnlyCurrentCultureConverter)));

        builder.Services
            .AddOpenApiDocument(settings =>
            {
                settings.DocumentName = "AspNetExample";
                settings.Title = "AspNetExample";
                settings.Description = "API documentation";
                settings.OperationProcessors.Insert(0, new OnlyApiOperationProcessor());
            });

        builder.Services.AddSingleton<CheckUserRolesMiddleware>();
        builder.Services.AddSingleton<ApiExceptionHandlerMiddleware>();

        AspNetExampleModule.RegisterDependencies(builder.Services, builder.Configuration);

        builder.Host.UseNLog();

        var app = builder.Build();

        InitializeApplicationContextAsync(app)
            .FireAndForgetSafeAsync();

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseMiddleware<ApiExceptionHandlerMiddleware>();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<CheckUserRolesMiddleware>();

        app.MapControllerRoute("default", "{controller=Home}/{action=Index}");

        app.Run();
    }

    private static async Task InitializeApplicationContextAsync(IHost app)
    {
        using var scope = app.Services.CreateScope();

        await scope.ServiceProvider
            .GetRequiredService<IApplicationContextStartupService>()
            .InitializeAsync();
    }
}