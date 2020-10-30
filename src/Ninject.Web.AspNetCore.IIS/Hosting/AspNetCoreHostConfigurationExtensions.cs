using Microsoft.AspNetCore.Hosting;

namespace Ninject.Web.AspNetCore.Hosting
{
	public static class AspNetCoreHostConfigurationExtensions
	{
		public static T UseIIS<T>(this T config)
			where T : IAspNetCoreHostConfiguration
		{
			config.ConfigureHostingModel(builder =>
			{
				builder
					.UseIIS()
					.UseIISIntegration();
			});
			return config;
		}
	}
}
