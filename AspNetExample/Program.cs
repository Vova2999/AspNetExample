using System.ComponentModel;
using AspNetExample.Common.Extensions;
using AspNetExample.Converters;
using AspNetExample.Domain.Entities;
using AspNetExample.Helpers;
using AspNetExample.Middlewares;
using AspNetExample.NSwag;
using AspNetExample.Services.Managers;
using AspNetExample.Services.Startup;
using AspNetExample.Services.Stores;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace AspNetExample;

public static class Program
{
    public static void Main(string[] args)
    {
        ComponentModelResourceManagerHelper.OverrideResourceManager();

        var builder = WebApplication.CreateBuilder(args);

        LogManager.Configuration = new NLogLoggingConfiguration(
            builder.Configuration.GetSection("NLog"));

        AspNetExampleModule.RegisterDependencies(builder.Services, builder.Configuration);

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = Constants.MultiAuthScheme;
                options.DefaultChallengeScheme = Constants.MultiAuthScheme;
                options.DefaultAuthenticateScheme = Constants.MultiAuthScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Constants.JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = Constants.JwtAudience,
                    ValidateLifetime = true,
                    IssuerSigningKey = Constants.GetJwtSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true
                };
            })
            .AddPolicyScheme(
                Constants.MultiAuthScheme,
                Constants.MultiAuthScheme,
                options =>
                {
                    options.ForwardDefaultSelector = context =>
                        context.Request.Path.StartsWithSegments("/api")
                            ? JwtBearerDefaults.AuthenticationScheme
                            : IdentityConstants.ApplicationScheme;
                });

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

                settings.AddSecurity(
                    JwtBearerDefaults.AuthenticationScheme,
                    [],
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = OpenApiSecuritySchemeType.Http,
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Description = "Введите JWT токен (префикс 'Bearer' добавится автоматически)",
                        BearerFormat = "JWT",
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        ExtensionData = new Dictionary<string, object?> { ["x-bearer-prefix"] = true }
                    });

                settings.OperationProcessors.Add(new OperationSecurityScopeProcessor(JwtBearerDefaults.AuthenticationScheme));

                settings.PostProcess = document =>
                {
                    if (document.Components.SecuritySchemes.TryGetValue(JwtBearerDefaults.AuthenticationScheme, out var securityScheme))
                        securityScheme.Scheme = JwtBearerDefaults.AuthenticationScheme;
                };
            });

        builder.Services.AddSingleton<SwaggerAuthorizedMiddleware>();
        builder.Services.AddSingleton<ApiExceptionHandlerMiddleware>();
        builder.Services.AddSingleton<DatabaseCheckUserRolesMiddleware>();

        builder.Host.UseNLog();

        var app = builder.Build();

        InitializeApplicationContextAsync(app)
            .FireAndForgetSafeAsync();

        if (!app.Environment.IsDevelopment())
            app.UseExceptionHandler("/Home/Error");

        app.UseMiddleware<ApiExceptionHandlerMiddleware>();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<DatabaseCheckUserRolesMiddleware>();

        app.UseMiddleware<SwaggerAuthorizedMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }

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