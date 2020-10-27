using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Ninject.Web.AspNetCore.Hosting
{
	public class AspNetCoreHostConfiguration
	{
		private Type _customStartup;
		private Action<KestrelServerOptions> _configureKestrelAction;

		internal Func<IWebHostBuilder> WebHostBuilderFactory { get; private set; }

		public AspNetCoreHostConfiguration(string[] cliArgs = null)
		{
			WebHostBuilderFactory = () => WebHost.CreateDefaultBuilder(cliArgs); ;
		}

		internal void ConfigureWebHostBuilder(Func<IWebHostBuilder> webHostBuilderFactory)
		{
			WebHostBuilderFactory = webHostBuilderFactory;
		}

		internal void ConfigureStartupType(Type startupType)
		{
			if (!typeof(AspNetCoreStartupBase).IsAssignableFrom(startupType))
			{
				throw new ArgumentException("startup type must inherit from " + nameof(AspNetCoreStartupBase));
			}
			_customStartup = startupType;
		}

		internal void ConfigureKestrel(Action<KestrelServerOptions> configureAction)
		{
			_configureKestrelAction = configureAction;
		}

		internal virtual void Apply(IWebHostBuilder builder)
		{
			ApplyHostingModel(builder);

			ApplyStartup(builder);
		}

		protected virtual void ApplyHostingModel(IWebHostBuilder builder)
		{
			if (_configureKestrelAction != null)
			{
				builder.UseKestrel(_configureKestrelAction);
			}
		}

		protected void ApplyStartup(IWebHostBuilder builder)
		{
			var startupType = _customStartup;
			if (startupType == null)
			{
				startupType = typeof(EmptyStartup);
			}

			builder.UseStartup(startupType);
		}
	}
}
