using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Ninject.Web.WebApiCore.Hosting
{
	public class WebApiHostConfiguration
	{
		private Type _customStartup;
		private Action<KestrelServerOptions> _configureKestrelAction;

		internal string[] CliArgs { get; private set; }

		public WebApiHostConfiguration(string[] cliArgs = null)
		{
			CliArgs = cliArgs;
		}

		internal void ConfigureStartupType(Type startupType)
		{
			if (!typeof(WebApiStartupBase).IsAssignableFrom(startupType))
			{
				throw new ArgumentException("startup type must inherit from " + nameof(WebApiStartupBase));
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
				startupType = typeof(DefaultWebApiStartup);
			}

			builder.UseStartup(startupType);
		}
	}
}
