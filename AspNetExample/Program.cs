using AspNetExample.Helpers;
using AspNetExample.Middlewares;
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

		builder.Services.AddTransient<ApiExceptionHandlerMiddleware>();
		AspNetExampleModule.RegisterDependencies(builder.Services);

		builder.Host.UseNLog();

		var app = builder.Build();

		if (!app.Environment.IsDevelopment())
			app.UseExceptionHandler("/Home/Error");

		app.UseMiddleware<ApiExceptionHandlerMiddleware>();

		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthorization();

		app.MapControllerRoute("default", "{controller=Home}/{action=Index}");

		app.Run();
	}
}