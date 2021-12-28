using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ninject.Web.AspNetCore
{
	public class NinjectServiceProviderBuilder
	{
		private readonly AspNetCoreKernel _kernel;
		private readonly IServiceCollection _services;

		public NinjectServiceProviderBuilder(AspNetCoreKernel kernel, IServiceCollection services)
		{
			_kernel = kernel;
			_services = services;
		}

		public IServiceProvider Build()
		{
			_kernel.Bind<IServiceProvider>().ToConstant(_kernel.RootScope.ServiceProvider);
#if NET6_0_OR_GREATER
			_kernel.Bind<IServiceProviderIsService>().To<NinjectServiceProviderIsService>().InSingletonScope();
#endif

			var adapter = new ServiceCollectionAdapter();
			adapter.Populate(_kernel, _services);

			return _kernel.RootScope.ServiceProvider;
		}

	}
}
