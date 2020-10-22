using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.WebApiCore
{
	public class NInjectServiceProviderFactory : IServiceProviderFactory<NInjectServiceProviderBuilder>
	{

		private readonly Action<IKernel> _configurationAction;
		private readonly IKernel _kernel;


		public NInjectServiceProviderFactory(Action<IKernel> configurationAction = null) : this (new StandardKernel(), configurationAction)
		{
		}

		public NInjectServiceProviderFactory(IKernel kernel, Action<IKernel> configurationAction = null)
		{
			_kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
			_configurationAction = configurationAction ?? (k => { });
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
