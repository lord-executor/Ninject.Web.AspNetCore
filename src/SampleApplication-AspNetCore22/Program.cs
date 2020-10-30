using Microsoft.AspNetCore.Hosting;
using Ninject;
using Ninject.Web.AspNetCore.Hosting;
using Ninject.Web.Common.SelfHost;
using System;
using System.Linq;

namespace SampleApplication_AspNetCore22
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// IIS just starts the server without any arguments, so it is the default here
			var model = args.FirstOrDefault(arg => arg.StartsWith("--use"))?.Substring(5) ?? "IIS";
			var hostConfiguration = new AspNetCoreHostConfiguration(args)
					.UseStartup(typeof(Startup));

			switch (model)
			{
				case "Kestrel":
				case "IISExpress":
					hostConfiguration.UseKestrel();
					break;

				case "HttpSys":
					hostConfiguration.UseHttpSys();
					break;

				case "IIS":
					hostConfiguration.UseIIS();
					break;

				default:
					throw new ArgumentException($"Unknown hosting model '{model}'");
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
