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
			// Simple (and probably unreliable) IIS detection mechanism
			var model = Environment.GetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES") == "Microsoft.AspNetCore.Server.IISIntegration" ? "IIS" : null;
			// The hosting model can be explicitly configured with the SERVER_HOSTING_MODEL environment variable.
			// See https://www.andrecarlucci.com/en/setting-environment-variables-for-asp-net-core-when-publishing-on-iis/ for
			// setting the variable in IIS.
			model = Environment.GetEnvironmentVariable("SERVER_HOSTING_MODEL") ?? model;
			// Command line arguments have higher precedence than environment variables
			model = args.FirstOrDefault(arg => arg.StartsWith("--use"))?.Substring(5) ?? model;

			var hostConfiguration = new AspNetCoreHostConfiguration(args)
					.UseStartup<Startup>();

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
