using Microsoft.Extensions.DependencyInjection;
using Ninject.Web.AspNetCore.Hosting;
using System;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class TestKernelContext
	{

		protected IKernel CreateKernel(IServiceCollection collection, AspNetCoreHostConfiguration configuration = null)
		{
			var kernel = new StandardKernel();
			kernel.Load(typeof(AspNetCoreApplicationPlugin).Assembly);
			kernel.Bind<IServiceProvider>().ToConstant(new NInjectServiceProvider(kernel));
			kernel.Bind<AspNetCoreHostConfiguration>().ToConstant(configuration ?? new AspNetCoreHostConfiguration());
			kernel.Populate(collection);			
			return kernel;
		}
		

	}
}
