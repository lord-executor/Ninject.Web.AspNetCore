﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Ninject;
using Ninject.Web.AspNetCore;
using Ninject.Web.AspNetCore.Hosting;
using Ninject.Web.Common;
using Ninject.Web.Common.SelfHost;
using SampleApplication.Controllers;
using SampleApplication.Service;
using System;
using System.Linq;
using System.Reflection;

namespace SampleApplication
{
	public class Program
	{
		public static void Main(string[] args)
		{
			// The hosting model can be explicitly configured with the SERVER_HOSTING_MODEL environment variable.
			// See https://www.andrecarlucci.com/en/setting-environment-variables-for-asp-net-core-when-publishing-on-iis/ for
			// setting the variable in IIS.
			var model = Environment.GetEnvironmentVariable("SERVER_HOSTING_MODEL");
			// Command line arguments have higher precedence than environment variables
			model = args.FirstOrDefault(arg => arg.StartsWith("--use"))?.Substring(5) ?? model;

			var hostConfiguration = new AspNetCoreHostConfiguration(args)
					.UseStartup<Startup>()
					.UseWebHostBuilder(CreateWebHostBuilder)
					.BlockOnStart();

			switch (model)
			{
				case "Kestrel":
					hostConfiguration.UseKestrel();
					break;

				case "HttpSys":
#pragma warning disable CA1416 // Validate platform compatibility
					hostConfiguration.UseHttpSys();
#pragma warning restore CA1416 // Validate platform compatibility
					break;

				case "IIS":
				case "IISExpress":
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
			var settings = new NinjectSettings();
			// Unfortunately, in .NET Core projects, referenced NuGet assemblies are not copied to the output directory
			// in a normal build which means that the automatic extension loading does not work _reliably_ and it is
			// much more reasonable to not rely on that and load everything explicitly.
			settings.LoadExtensions = false;

			var kernel = new AspNetCoreKernel(settings);
			kernel.DisableAutomaticSelfBinding();

			kernel.Load(typeof(AspNetCoreHostConfiguration).Assembly);
			kernel.Load(Assembly.GetExecutingAssembly());

			kernel.Bind<Lazy<IModelMetadataProvider>>().ToMethod(x =>
				new Lazy<IModelMetadataProvider>(() => x.Kernel.Get<IModelMetadataProvider>()));
			kernel.Bind<PublishInstructionApplicationModelConvention>().ToSelf().InTransientScope();
			kernel.Bind<IConfigureOptions<MvcOptions>>().To<PublishInstructionApplicationModelConfiguration>().InTransientScope();

			kernel.Bind<HomeController>().ToSelf().InRequestScope();

			return kernel;
		}

		public static IWebHostBuilder CreateWebHostBuilder()
		{
			return new DefaultWebHostConfiguration(null)
				.ConfigureAll()
				.GetBuilder();
		}
	}
}
