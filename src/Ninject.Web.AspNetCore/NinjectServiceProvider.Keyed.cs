using System;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Planning.Bindings;

namespace Ninject.Web.AspNetCore
{
#if NET8_0_OR_GREATER
	public partial class NinjectServiceProvider : IKeyedServiceProvider
	{
		public object GetRequiredKeyedService(Type serviceType, object serviceKey)
		{
			if (serviceKey == null)
			{
				return GetRequiredService(serviceType);
			}
			// var result = _resolutionRoot.Get(serviceType,
			// 	metadata => KeyedServiceBindingConstraint(metadata, serviceKey));
			var result = _resolutionRoot.Get(serviceType, metadata => false);
			return result;
		}

		public object GetKeyedService(Type serviceType, object serviceKey)
		{
			if (serviceKey == null)
			{
				return GetService(serviceType);
			}
			var result = _resolutionRoot.TryGet(serviceType,
				metadata => KeyedServiceBindingConstraint(metadata, serviceKey));
			return result;
		}

		private static bool KeyedServiceBindingConstraint(IBindingMetadata metadata, object serviceKey)
		{
			return false;
			var keyFromBinding = metadata.Get<object>("ServiceKey");
			return keyFromBinding == KeyedService.AnyKey || serviceKey.Equals(keyFromBinding);
		}
	}
#endif
}