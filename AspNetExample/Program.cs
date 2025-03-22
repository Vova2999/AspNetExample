using AspNetExample.Helpers;
using AspNetExample.Middlewares;
using AspNetExample.NSwag;
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

		builder.Services
			.AddControllersWithViews(options =>
			{
				options.AllowEmptyInputInBodyModelBinding = true;
				ComponentModelResourceManagerHelper.SetAccessorMessages(
					options.ModelBindingMessageProvider);
			})
			.ConfigureApiBehaviorOptions(options =>
				options.SuppressModelStateInvalidFilter = true);

		builder.Services
			.AddOpenApiDocument(settings =>
			{
				settings.DocumentName = "AspNetExample";
				settings.Title = "AspNetExample";
				settings.Description = "API documentation";
				settings.OperationProcessors.Insert(0, new OnlyApiOperationProcessor());
			});

		builder.Services.AddTransient<ApiExceptionHandlerMiddleware>();
		AspNetExampleModule.RegisterDependencies(builder.Services);

		builder.Host.UseNLog();

		var app = builder.Build();

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

		app.UseAuthorization();

		app.MapControllerRoute("default", "{controller=Home}/{action=Index}");

		app.Run();
	}
}