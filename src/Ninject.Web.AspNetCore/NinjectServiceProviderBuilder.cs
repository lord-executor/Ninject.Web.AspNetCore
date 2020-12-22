using Microsoft.Extensions.DependencyInjection;
using System;

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

			var adapter = new ServiceCollectionAdapter();
			adapter.Populate(_kernel, _services);

			return result;
		}

	}
}
