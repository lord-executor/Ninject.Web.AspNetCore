using Microsoft.AspNetCore.Hosting;
using Ninject;
using Ninject.Web.AspNetCore.Hosting;
using Ninject.Web.Common.SelfHost;
using System.Linq;

namespace SampleApplication_AspNetCore22
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// When no hosting model is selected with UseKestrel or UseHttpSys, the default from ASP.NET Core
			// is to run in Kestrel when the program is started normally and to run in IIS in-process mode
			// when it detects that it is running inside of an IIS process. So technically, UseKestrel is
			// optional and does not need to be called unless you want to configure it.
			var model = args.FirstOrDefault(arg => arg.StartsWith("--use"))?.Substring(5) ?? "IIS";
			var hostConfiguration = new AspNetCoreHostConfiguration(args)
					.UseStartup(typeof(Startup));

			if (model == "HttpSys")
			{
				hostConfiguration.UseHttpSys();
			}
			else if (model == "Kestrel")
			{
				hostConfiguration.UseKestrel();
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
