using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;

namespace Ninject.Web.AspNetCore.Hosting
{
	public static class AspNetCoreHostConfigurationExtensions
	{
		public static T UseWebHostBuilder<T>(this T config, IWebHostBuilder builder)
			where T : IAspNetCoreHostConfiguration
		{
			return config.UseWebHostBuilder(() => builder);
		}

		public static T UseWebHostBuilder<T>(this T config, Func<IWebHostBuilder> builderFactory)
			where T : IAspNetCoreHostConfiguration
		{
			config.ConfigureWebHostBuilder(builderFactory);
			return config;
		}

		public static T UseStartup<T>(this T config, Type startupType)
			where T : IAspNetCoreHostConfiguration
		{
			config.ConfigureStartupType(startupType);
			return config;
		}

		public static T UseKestrel<T>(this T config)
			where T : IAspNetCoreHostConfiguration
		{
			return config.UseKestrel(_ => { });
		}

		public static T UseKestrel<T>(this T config, Action<KestrelServerOptions> configureAction)
			where T : IAspNetCoreHostConfiguration
		{
			config.ConfigureHostingModel(builder =>
			{
				builder.UseKestrel(configureAction);
			});
			return config;
		}
	}
}
