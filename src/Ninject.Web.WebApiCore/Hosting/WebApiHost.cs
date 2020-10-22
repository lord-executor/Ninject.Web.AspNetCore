using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
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
			var host = new WebHostBuilder()
				.ConfigureServices(s => { s.AddNinject(_kernel); });
			_configuration.Apply(host);

			var builder = host.Build();
			builder.Run();
		}
	}
}
