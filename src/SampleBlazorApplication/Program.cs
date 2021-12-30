using Ninject;
using Ninject.Web.AspNetCore;
using SampleBlazorApplication.Data;

namespace SampleBlazorApplication;

public class Program
{
	public static void Main(string[] args)
	{
		var kernel = CreateKernel();
		var builder = WebApplication.CreateBuilder(args);

		builder.Host.UseServiceProviderFactory(new NinjectServiceProviderFactory(kernel));

		// Add services to the container.
		builder.Services.AddRazorPages();
		builder.Services.AddServerSideBlazor();
		builder.Services.AddSingleton<WeatherForecastService>();

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
		}

		app.UseStaticFiles();

		app.UseRouting();

		app.MapBlazorHub();
		app.MapFallbackToPage("/_Host");

		app.Run();
	}

	public static AspNetCoreKernel CreateKernel()
	{
		var settings = new NinjectSettings();
		// Unfortunately, in .NET Core projects, referenced NuGet assemblies are not copied to the output directory
		// in a normal build which means that the automatic extension loading does not work _reliably_ and it is
		// much more reasonable to not rely on that and load everything explicitly.
		settings.LoadExtensions = false;

		var kernel = new AspNetCoreKernel(settings);
		kernel.DisableAutomaticSelfBinding();

		kernel.Load(new AspNetCoreModule());

		return kernel;
	}
}