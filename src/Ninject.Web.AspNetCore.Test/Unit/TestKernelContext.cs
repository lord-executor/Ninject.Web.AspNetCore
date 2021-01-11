using Microsoft.Extensions.DependencyInjection;
using Ninject.Web.AspNetCore.Hosting;
using System;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class TestKernelContext
	{
		protected AspNetCoreHostConfiguration DefaultConfiguration { get; set; } = new AspNetCoreHostConfiguration();

		protected NinjectSettings CreateDefaultSettings()
		{
			return new NinjectSettings() { LoadExtensions = false };
		}

		protected IKernel CreateKernel(IServiceCollection collection)
		{
			return CreateKernel(collection, CreateDefaultSettings(), DefaultConfiguration);
		}

		protected IKernel CreateKernel(IServiceCollection collection, AspNetCoreHostConfiguration configuration)
		{
			return CreateKernel(collection, CreateDefaultSettings(), configuration);
		}

		protected IKernel CreateKernel(IServiceCollection collection, NinjectSettings settings)
		{
			return CreateKernel(collection, settings, DefaultConfiguration);
		}

		protected IKernel CreateKernel(IServiceCollection collection, NinjectSettings settings, AspNetCoreHostConfiguration configuration)
		{
			var kernel = new AspNetCoreKernel(settings);
			kernel.Load(typeof(AspNetCoreApplicationPlugin).Assembly);
			kernel.Bind<IServiceProvider>().ToConstant(new NinjectServiceProvider(kernel));
			kernel.Bind<AspNetCoreHostConfiguration>().ToConstant(configuration);

			new ServiceCollectionAdapter().Populate(kernel, collection);
			return kernel;
		}


	}
}
