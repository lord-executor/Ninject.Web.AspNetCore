using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore
{
	public class NInjectServiceProviderFactory : IServiceProviderFactory<NInjectServiceProviderBuilder>
	{
		private readonly IKernel _kernel;


		public NInjectServiceProviderFactory() : this(new StandardKernel())
		{
		}

		public NInjectServiceProviderFactory(IKernel kernel)
		{
			_kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
		}

		public NInjectServiceProviderBuilder CreateBuilder(IServiceCollection services)
		{
			return new NInjectServiceProviderBuilder(_kernel, services);
		}

		public IServiceProvider CreateServiceProvider(NInjectServiceProviderBuilder containerBuilder)
		{
			if (containerBuilder == null) throw new ArgumentNullException(nameof(containerBuilder));

			return containerBuilder.Build();
		}
	}
}
