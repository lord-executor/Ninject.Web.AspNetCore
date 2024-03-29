﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Ninject.Web.Common.SelfHost;


namespace Ninject.Web.AspNetCore.Hosting
{
	public class AspNetCoreHost : INinjectSelfHost
	{
		private AspNetCoreHostConfiguration _configuration;
		private IKernel _kernel;

		public AspNetCoreHost(AspNetCoreHostConfiguration configuration, IKernel kernel)
		{
			_configuration = configuration;
			_kernel = kernel;
		}

		public void Start()
		{
			// The default web host builder takes care of
			// * Content and Web-root
			// * Loading appsettings.json files and environment variables configuration source
			// * Logging configuration (from appsettings.json)
			// * AllowedHosts configuration (from appsettings.json)
			var builder = _configuration.WebHostBuilderFactory()
				.ConfigureServices(s => { s.AddNinject(_kernel); });
			_configuration.Apply(builder);

			var host = builder.Build();

			if (_configuration == null || _configuration.BlockOnStart)
			{
				host.Run();
			}
			else
			{
				host.RunAsync(_configuration.CancellationToken);
			}
		}
	}
}
