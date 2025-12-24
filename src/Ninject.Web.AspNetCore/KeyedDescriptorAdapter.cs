using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore
{
#if NET8_0_OR_GREATER
	/// <summary>
	/// This ServiceDescriptorAdapter is used when ServiceDescriptor.IsKeyedService == true
	/// </summary>
	public class KeyedDescriptorAdapter : IDescriptorAdapter
	{

		private ServiceDescriptor _descriptor;

		public KeyedDescriptorAdapter(ServiceDescriptor descriptor)
		{
			_descriptor = descriptor;
		}

		public Type ImplementationType => _descriptor.KeyedImplementationType;
		public object ImplementationInstance => _descriptor.KeyedImplementationInstance;
		public bool UseServiceFactory => _descriptor.KeyedImplementationFactory != null;
		public object InstantiateFromServiceFactory(IServiceProvider provider)
		{
			return _descriptor.KeyedImplementationFactory(provider, _descriptor.ServiceKey);
		}

		public ServiceLifetime Lifetime => _descriptor.Lifetime;
	}
#endif
}