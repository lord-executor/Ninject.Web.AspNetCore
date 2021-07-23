using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ninject.Web.AspNetCore
{
	public class NinjectServiceProviderBuilder
	{
		private readonly IKernel _kernel;
		private readonly IServiceCollection _services;

		public NinjectServiceProviderBuilder(IKernel kernel, IServiceCollection services)
		{
			_kernel = kernel;
			_services = services;
		}

		public NinjectServiceProvider Build()
		{
			var scope = new NinjectServiceScope(_kernel, true);
			//var result = new NinjectServiceProvider(_kernel);
			
			_kernel.Bind<IServiceProvider>().ToConstant(scope.ServiceProvider); // needed for factory methods in IServiceCollection

			var adapter = new ServiceCollectionAdapter();
			adapter.Populate(_kernel, _services);

			return (NinjectServiceProvider)scope.ServiceProvider;
		}

	}
}
