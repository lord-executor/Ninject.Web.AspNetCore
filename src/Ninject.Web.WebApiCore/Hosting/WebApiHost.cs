using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Ninject.Web.Common.SelfHost;

namespace Ninject.Web.WebApiCore.Hosting
{
	public class WebApiHost : INinjectSelfHost
	{
		private WebApiHostConfiguration _configuration;
		private IKernel _kernel;

		public WebApiHost(WebApiHostConfiguration configuration, IKernel kernel)
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
			var host = WebHost.CreateDefaultBuilder(_configuration.CliArgs)
				.ConfigureServices(s => { s.AddNinject(_kernel); });
			_configuration.Apply(host);

			var builder = host.Build();
			builder.Run();
		}
	}
}
