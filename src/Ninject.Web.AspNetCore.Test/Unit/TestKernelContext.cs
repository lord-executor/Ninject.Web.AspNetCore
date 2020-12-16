using Microsoft.Extensions.DependencyInjection;
using Ninject.Web.AspNetCore.Hosting;
using System;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class TestKernelContext
	{

		protected IKernel CreateKernel(IServiceCollection collection, AspNetCoreHostConfiguration configuration = null)
		{
			var kernel = new StandardKernel(new NinjectSettings() { LoadExtensions = false });
			kernel.Load(typeof(AspNetCoreApplicationPlugin).Assembly);
			kernel.Bind<IServiceProvider>().ToConstant(new NinjectServiceProvider(kernel));
			kernel.Bind<AspNetCoreHostConfiguration>().ToConstant(configuration ?? new AspNetCoreHostConfiguration());

			new ServiceCollectionAdapter().Populate(kernel, collection);
			return kernel;
		}
		

	}
}
