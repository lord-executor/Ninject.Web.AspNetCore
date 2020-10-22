using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Syntax;
using Ninject.Web.Common;

namespace Ninject.Web.WebApiCore
{
	public static class KernelExtensions
	{
		public static void Populate(
			this IKernel kernel,
			IEnumerable<ServiceDescriptor> descriptors)
		{
			if (descriptors == null)
			{
				throw new ArgumentNullException(nameof(descriptors));
			}

			var adapters = kernel.GetAll<IPopulateAdapter>().ToList();

			foreach(var descriptor in descriptors)
			{
				if (UsedCustomBinding(kernel, adapters, descriptor))
				{
					continue;
				}

				kernel.Bind(descriptor.ServiceType).ConfigureImplementationAndLifecycle(descriptor);
			}

			foreach (var adapter in adapters)
			{
				adapter.AdaptAfterPopulate(kernel);
			}
		}

		private static bool UsedCustomBinding(IKernel kernel, List<IPopulateAdapter> adapters, ServiceDescriptor descriptor)
		{
			foreach (var adapter in adapters)
			{
				if (adapter.AdaptDescriptor(kernel, descriptor))
				{
					return true;
				}
			}

			return false;
		}

		public static IBindingNamedWithOrOnSyntax<T> ConfigureLifecycle<T>(
			this IBindingInSyntax<T> bindingInSyntax,
			ServiceLifetime lifecycleKind)
		{
			switch (lifecycleKind)
			{
				case ServiceLifetime.Singleton:
					return bindingInSyntax.InSingletonScope();
				case ServiceLifetime.Scoped:
					return bindingInSyntax.InRequestScope();
                case ServiceLifetime.Transient:
					return bindingInSyntax.InTransientScope();
				default:
	                throw new NotSupportedException();
            }
		}

		public static IBindingNamedWithOrOnSyntax<T> ConfigureImplementationAndLifecycle<T>(this IBindingToSyntax<T> bindingToSyntax,
			ServiceDescriptor descriptor) where T: class
		{
			IBindingNamedWithOrOnSyntax<T> result;
			if (descriptor.ImplementationType != null)
			{
				result = bindingToSyntax.To(descriptor.ImplementationType).ConfigureLifecycle(descriptor.Lifetime);
			}
			else if (descriptor.ImplementationFactory != null)
			{

				result = bindingToSyntax.ToMethod(context
					=>
				{
					var provider = context.Kernel.Get<IServiceProvider>();
					return descriptor.ImplementationFactory(provider) as T;
				}).ConfigureLifecycle(descriptor.Lifetime);
			}
			else
			{
				// use ToMethod here as ToConstant has the wrong return type.
				result = bindingToSyntax.ToMethod(context => descriptor.ImplementationInstance as T).InSingletonScope();
			}

			return result;
		}

	}


    
}
