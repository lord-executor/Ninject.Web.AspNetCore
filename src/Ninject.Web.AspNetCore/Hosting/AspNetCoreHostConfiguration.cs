using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading;

namespace Ninject.Web.AspNetCore.Hosting
{
	public class AspNetCoreHostConfiguration : IAspNetCoreHostConfiguration
	{
		private Type _customStartup;
		private Action<IWebHostBuilder> _hostingModelConfigurationAction;

		internal Func<IWebHostBuilder> WebHostBuilderFactory { get; private set; }
		internal bool BlockOnStart { get; private set; } = false;
		internal CancellationToken CancellationToken { get; private set; }
		internal Type CustomControllerActivator { get; private set; }

		public AspNetCoreHostConfiguration(string[] cliArgs = null)
		{
			_customStartup = typeof(EmptyStartup);
			WebHostBuilderFactory = () => new DefaultWebHostConfiguration(cliArgs).ConfigureAll().GetBuilder();
		}

		void IAspNetCoreHostConfiguration.ConfigureWebHostBuilder(Func<IWebHostBuilder> webHostBuilderFactory)
		{
			WebHostBuilderFactory = webHostBuilderFactory;
		}

		void IAspNetCoreHostConfiguration.ConfigureStartupType(Type startupType)
		{
			if (!typeof(AspNetCoreStartupBase).IsAssignableFrom(startupType))
			{
				throw new ArgumentException("Startup type must inherit from " + nameof(AspNetCoreStartupBase));
			}
			_customStartup = startupType;
		}

		void IAspNetCoreHostConfiguration.ConfigureHostingModel(Action<IWebHostBuilder> configureAction)
		{
			_hostingModelConfigurationAction = configureAction;
		}

		void IAspNetCoreHostConfiguration.ConfigureStartupBehavior(bool blockOnStart, CancellationToken cancellationToken)
		{
			BlockOnStart = blockOnStart;
			CancellationToken = cancellationToken;
		}

		internal virtual void Apply(IWebHostBuilder builder)
		{
			_hostingModelConfigurationAction?.Invoke(builder);
			builder.UseStartup(_customStartup);
		}

		void IAspNetCoreHostConfiguration.ConfigureCustomControllerActivator(Type controllerActivatorType)
		{
			CustomControllerActivator = controllerActivatorType;
		}
	}
}
