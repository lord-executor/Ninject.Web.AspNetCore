using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;

namespace Ninject.Web.WebApiCore.Hosting
{
	public static class WebApiHostConfigurationExtensions
	{
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
