using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore
{
	public class NinjectServiceProviderFactory : IServiceProviderFactory<NinjectServiceProviderBuilder>
	{
		private readonly IKernel _kernel;


		public NinjectServiceProviderFactory() : this(new AspNetCoreKernel())
		{
		}

		public NinjectServiceProviderFactory(IKernel kernel)
		{
			_kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
			if (!(kernel is AspNetCoreKernel))
			{
				throw new ArgumentException($"The kernel used for ASP.NET Core must be derived from AspNetCoreKernel", nameof(kernel));
			}
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
