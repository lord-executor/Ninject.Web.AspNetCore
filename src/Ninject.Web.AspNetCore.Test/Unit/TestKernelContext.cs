using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class TestKernelContext
	{

		protected IKernel CreateKernel(IServiceCollection collection)
		{
			var kernel = new StandardKernel();			
			kernel.Load(typeof(AspNetCoreApplicationPlugin).Assembly);
			kernel.Bind<IServiceProvider>().ToConstant(new NInjectServiceProvider(kernel));
			kernel.Populate(collection);			
			return kernel;
		}
		

	}
}
