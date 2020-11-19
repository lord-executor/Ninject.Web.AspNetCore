using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;
using System.Threading;

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

		public static AspNetCoreHostConfiguration UseStartup<TStartup>(this AspNetCoreHostConfiguration config)
		{
			return config.UseStartup(typeof(TStartup));
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
				builder.UseKestrel((builderContext, options) => {
					// Defaults from Microsoft.AspNetCore.WebHost.CreateDefaultBuilder
					options.Configure(builderContext.Configuration.GetSection("Kestrel"));
					configureAction(options);
				});
			});
			return config;
		}

		public static T BlockOnStart<T>(this T config, bool blockOnStart = true)
			where T : IAspNetCoreHostConfiguration
		{
			config.ConfigureStartupBehavior(blockOnStart, default);
			return config;
		}

		public static T UseCancellationToken<T>(this T config, CancellationToken token)
			where T : IAspNetCoreHostConfiguration
		{
			config.ConfigureStartupBehavior(false, token);
			return config;
		}

		public static T UseCustomControllerActivator<T>(this T config, Type customControllerActivator)
			where T : IAspNetCoreHostConfiguration
		{
			config.ConfigureCustomControllerActivator(customControllerActivator);
			return config;
		}
	}
}
