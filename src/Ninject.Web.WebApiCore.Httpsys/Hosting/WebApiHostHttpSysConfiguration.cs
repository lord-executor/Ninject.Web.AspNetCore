using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using System;

namespace Ninject.Web.WebApiCore.Hosting
{
	public class WebApiHostHttpSysConfiguration : WebApiHostConfiguration
	{
		private Action<HttpSysOptions> _configureHttpSysAction;

		public WebApiHostHttpSysConfiguration(string[] cliArgs = null)
			: base(cliArgs)
		{
		}

		public WebApiHostHttpSysConfiguration UseHttpSys(Action<HttpSysOptions> configureAction)
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
