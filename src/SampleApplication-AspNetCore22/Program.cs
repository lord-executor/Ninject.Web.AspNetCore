using Microsoft.AspNetCore.Hosting;
using Ninject;
using Ninject.Web.AspNetCore.Hosting;
using Ninject.Web.Common.SelfHost;

namespace SampleApplication_AspNetCore22
{
	public class Program
	{
		public static void Main(string[] args)
		{
			object hostConfiguration;

			if (args.Length > 0 && args[0] == "--useHttpSys")
			{
				hostConfiguration = new AspNetCoreHostHttpSysConfiguration(args)
					.UseWebHostBuilder(CreateWebHostBuilder)
					.UseStartup(typeof(Startup))
					.UseHttpSys(_ => { });
			}
			else
			{
				hostConfiguration = new AspNetCoreHostConfiguration(args)
					.UseStartup(typeof(Startup))
					.UseKestrel();
			}

			var host = new NinjectSelfHostBootstrapper(CreateKernel, hostConfiguration);
			host.Start();
		}

		public static IKernel CreateKernel()
		{
			var kernel = new StandardKernel();
			return kernel;
		}

		public static IWebHostBuilder CreateWebHostBuilder()
		{
			return new WebHostBuilder();
		}
	}
}
