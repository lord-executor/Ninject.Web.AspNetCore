using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore
{
	public class NinjectServiceProviderBuilder
	{
		private readonly IServiceCollection _services;
		private readonly IKernel _kernel;

		public NinjectServiceProviderBuilder(IKernel kernel, IServiceCollection services)
		{
			_kernel = kernel;
			_services = services;
		}

		public NinjectServiceProvider Build()
		{
			var result = new NinjectServiceProvider(_kernel);
			_kernel.Bind<IServiceProvider>().ToConstant(result); // needed for factory methods in IServiceCollection
			_kernel.Populate(_services);
			return result;
		}

	}
}
