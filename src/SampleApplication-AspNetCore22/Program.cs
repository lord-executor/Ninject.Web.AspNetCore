using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Ninject;
using Ninject.Web.Common.SelfHost;
using Ninject.Web.WebApiCore.Hosting;

namespace SampleApplication_AspNetCore22
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var options = new WebApiHostConfiguration(args)
				.UseWebHostBuilder(CreateWebHostBuilder)
				.UseStartup(typeof(Startup))
				.UseKestrel();

			var host = new NinjectSelfHostBootstrapper(CreateKernel, options);
			host.Start();
		}

		public static IKernel CreateKernel()
		{
			var kernel = new StandardKernel();
			return kernel;
		}

		public static IWebHostBuilder CreateWebHostBuilder()
		{
			return WebHost.CreateDefaultBuilder();
		}
	}
}
