using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore
{
	public class NinjectServiceProviderFactory : IServiceProviderFactory<NinjectServiceProviderBuilder>
	{
		private readonly AspNetCoreKernel _kernel;


		public NinjectServiceProviderFactory() : this(new AspNetCoreKernel())
		{
		}

		public NinjectServiceProviderFactory(AspNetCoreKernel kernel)
		{
			_kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
		}

		public NinjectServiceProviderBuilder CreateBuilder(IServiceCollection services)
		{
			return new NinjectServiceProviderBuilder(_kernel, services);
		}

		public IServiceProvider CreateServiceProvider(NinjectServiceProviderBuilder containerBuilder)
		{
			if (containerBuilder == null) throw new ArgumentNullException(nameof(containerBuilder));

			return containerBuilder.Build();
		}
	}
}
