﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Ninject.Web.WebApiCore.Hosting;

namespace Ninject.Web.WebApiCore.Hosting
{
	public class WebApiHostHttpSysConfiguration : WebApiHostConfiguration
	{
		private Action<HttpSysOptions> _configureHttpSysAction;

		public new WebApiHostHttpSysConfiguration UseStartup<TStartup>() where TStartup : WebApiStartupBase
		{
			return UseStartup(typeof(TStartup));
		}

		public new WebApiHostHttpSysConfiguration UseStartup(Type startupType)
		{
			ConfigureStartupType(startupType);
			return this;
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
