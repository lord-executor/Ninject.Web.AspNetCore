using System;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Activation;

namespace Ninject.Web.AspNetCore
{
	/// <summary>
	/// This ServiceDescriptorAdapter is used when ServiceDescriptor.IsKeyedService == false
	/// This was always the case before .NET 8.0
	/// </summary>
	public class DefaultDescriptorAdapter : IDescriptorAdapter
	{
		private ServiceDescriptor _descriptor;

		public DefaultDescriptorAdapter(ServiceDescriptor descriptor)
		{
			_descriptor = descriptor;
		}

		public Type ImplementationType => _descriptor.ImplementationType;
		public object ImplementationInstance => _descriptor.ImplementationInstance;
		public bool UseServiceFactory => _descriptor.ImplementationFactory != null;
		public object InstantiateFromServiceFactory(IServiceProvider provider, IContext context)
		{
			return _descriptor.ImplementationFactory(provider);
		}
		public ServiceLifetime Lifetime => _descriptor.Lifetime;
	}
}