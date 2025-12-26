using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Ninject.Web.AspNetCore
{
#if NET6_0_OR_GREATER
	public class NinjectServiceProviderIsService : IServiceProviderIsService
#if NET8_0_OR_GREATER
	, IServiceProviderIsKeyedService
#endif
	{
		private readonly IKernel _kernel;

		public NinjectServiceProviderIsService(IKernel kernel)
		{
			_kernel = kernel;
		}

		public bool IsService(Type serviceType)
		{
			// IsService should only return true if the type can actually be resolved to a service
			// and open generic types cannot. Except for IEnumerable<T> which should return true
			// in ANY case (see DependencyInjectionSpecificationTests.IEnumerableWithIsServiceAlwaysReturnsTrue)
			if (serviceType.IsGenericTypeDefinition)
			{
				return false;
			}

			if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				return true;
			}

			return _kernel.CanResolve(serviceType);
		}
#if NET8_0_OR_GREATER
		public bool IsKeyedService(Type serviceType, object serviceKey)
		{
			// IsService should only return true if the type can actually be resolved to a service
			// and open generic types cannot. Except for IEnumerable<T> which should return true
			// in ANY case (see DependencyInjectionSpecificationTests.IEnumerableWithIsServiceAlwaysReturnsTrue)
			if (serviceType.IsGenericTypeDefinition)
			{
				return false;
			}

			if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				return true;
			}

			return _kernel.CanResolve(serviceType, metadata =>
				metadata.Get<ServiceKey>(nameof(ServiceKey))?.Key == serviceKey);
		}
#endif
	}
#endif
}
