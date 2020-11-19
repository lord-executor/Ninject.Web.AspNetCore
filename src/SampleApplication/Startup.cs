using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ninject.Web.AspNetCore;
using Ninject.Web.AspNetCore.Hosting;

namespace SampleApplication_AspNetCore
{
	public class Startup : AspNetCoreStartupBase
	{
		public Startup(IConfiguration configuration, IServiceProviderFactory<NinjectServiceProviderBuilder> providerFactory)
			: base(providerFactory)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public override void ConfigureServices(IServiceCollection services)
		{
			base.ConfigureServices(services);

			services.AddControllersWithViews();
		}

		public override void Configure(IApplicationBuilder app)
		{
			var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
