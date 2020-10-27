using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using System;

namespace Ninject.Web.AspNetCore.Hosting
{
	public class AspNetCoreHostHttpSysConfiguration : AspNetCoreHostConfiguration
	{
		private Action<HttpSysOptions> _configureHttpSysAction;

		public AspNetCoreHostHttpSysConfiguration(string[] cliArgs = null)
			: base(cliArgs)
		{
		}

		public AspNetCoreHostHttpSysConfiguration UseHttpSys(Action<HttpSysOptions> configureAction)
		{
			_configureHttpSysAction = configureAction;
			return this;
		}

		protected override void ApplyHostingModel(IWebHostBuilder builder)
		{
			if (_configureHttpSysAction != null)
			{
				builder.UseHttpSys(_configureHttpSysAction);
			}
		}

	}
}
