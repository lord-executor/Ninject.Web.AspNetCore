using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Activation;
using Ninject.Web.AspNetCore.Parameters;

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
		public object InstantiateFromServiceFactory(IServiceProvider provider, IContext context)
		{
			object serviceKey = _descriptor.ServiceKey;
			var keyParameter = context.Parameters.LastOrDefault(x => x is ServiceKeyParameter) as ServiceKeyParameter;
			if (keyParameter != null)
			{
				serviceKey = keyParameter.ServiceKey;
			}
			return _descriptor.KeyedImplementationFactory(provider, serviceKey);
		}

		public ServiceLifetime Lifetime => _descriptor.Lifetime;
	}
#endif
}