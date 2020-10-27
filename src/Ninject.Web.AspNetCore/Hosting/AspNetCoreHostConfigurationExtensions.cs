using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;

namespace Ninject.Web.AspNetCore.Hosting
{
	public static class AspNetCoreHostConfigurationExtensions
	{
		public static T UseWebHostBuilder<T>(this T config, IWebHostBuilder builder)
			where T : AspNetCoreHostConfiguration
		{
			return config.UseWebHostBuilder(() => builder);
		}

		public static T UseWebHostBuilder<T>(this T config, Func<IWebHostBuilder> builderFactory)
			where T : AspNetCoreHostConfiguration
		{
			config.ConfigureWebHostBuilder(builderFactory);
			return config;
		}

		public static T UseStartup<T>(this T config, Type startupType)
			where T : AspNetCoreHostConfiguration
		{
			config.ConfigureStartupType(startupType);
			return config;
		}

		public static T UseKestrel<T>(this T config)
			where T : AspNetCoreHostConfiguration
		{
			return config.UseKestrel(_ => { });
		}

		public static T UseKestrel<T>(this T config, Action<KestrelServerOptions> configureAction)
			where T : AspNetCoreHostConfiguration
		{
			config.ConfigureKestrel(configureAction);
			return config;
		}
	}
}
