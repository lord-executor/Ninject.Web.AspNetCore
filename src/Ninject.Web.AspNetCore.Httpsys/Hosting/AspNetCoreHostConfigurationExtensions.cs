using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using System;

namespace Ninject.Web.AspNetCore.Hosting
{
	public static class AspNetCoreHostConfigurationExtensions
	{
		public static T UseHttpSys<T>(this T config)
			where T : IAspNetCoreHostConfiguration
		{
			return config.UseHttpSys(_ => { });
		}

		public static T UseHttpSys<T>(this T config, Action<HttpSysOptions> configureAction)
			where T : IAspNetCoreHostConfiguration
		{
			config.ConfigureHostingModel(builder =>
			{
				builder.UseHttpSys(configureAction);
			});
			return config;
		}
	}
}
