using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;

namespace Ninject.Web.WebApiCore.Hosting
{
	public static class WebApiHostConfigurationExtensions
	{
		public static T UseWebHostBuilder<T>(this T config, IWebHostBuilder builder)
			where T : WebApiHostConfiguration
		{
			return config.UseWebHostBuilder(() => builder);
		}

		public static T UseWebHostBuilder<T>(this T config, Func<IWebHostBuilder> builderFactory)
			where T : WebApiHostConfiguration
		{
			config.ConfigureWebHostBuilder(builderFactory);
			return config;
		}

		public static T UseStartup<T>(this T config, Type startupType)
			where T : WebApiHostConfiguration
		{
			config.ConfigureStartupType(startupType);
			return config;
		}

		public static T UseKestrel<T>(this T config)
			where T : WebApiHostConfiguration
		{
			return config.UseKestrel(_ => { });
		}

		public static T UseKestrel<T>(this T config, Action<KestrelServerOptions> configureAction)
			where T : WebApiHostConfiguration
		{
			config.ConfigureKestrel(configureAction);
			return config;
		}
	}
}
