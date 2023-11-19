using Microsoft.Extensions.DependencyInjection;
using Ninject.Syntax;
using Ninject.Web.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ninject.Web.AspNetCore
{
	public class ServiceCollectionAdapter
	{
		private readonly IDictionary<Type, BindingIndex> _bindingIndexMap = new Dictionary<Type, BindingIndex>();

		public void Populate(IKernel kernel, IServiceCollection serviceCollection)
		{
			if (serviceCollection == null)
			{
				throw new ArgumentNullException(nameof(serviceCollection));
			}

			var adapters = kernel.GetAll<IPopulateAdapter>().ToList();
			var bindingIndex = new BindingIndex();

			foreach (var descriptor in serviceCollection)
			{
				if (UsedCustomBinding(kernel, adapters, descriptor))
				{
					continue;
				}

				ConfigureImplementationAndLifecycle(kernel.Bind(descriptor.ServiceType), descriptor, bindingIndex);
			}

			foreach (var adapter in adapters)
			{
				adapter.AdaptAfterPopulate(kernel);
			}
		}

		private bool UsedCustomBinding(IKernel kernel, List<IPopulateAdapter> adapters, ServiceDescriptor descriptor)
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

		private IBindingWithOrOnSyntax<T> ConfigureImplementationAndLifecycle<T>(
			IBindingToSyntax<T> bindingToSyntax,
			ServiceDescriptor descriptor,
			BindingIndex bindingIndex) where T : class
		{
			IBindingNamedWithOrOnSyntax<T> result;

#if NET8_0_OR_GREATER
			if (descriptor.IsKeyedService)
			{
				if (descriptor.KeyedImplementationType != null)
				{
					result = ConfigureLifecycle(bindingToSyntax.To(descriptor.KeyedImplementationType), descriptor.Lifetime);
				}
				else if (descriptor.KeyedImplementationFactory != null)
				{
					result = ConfigureLifecycle(bindingToSyntax.ToMethod(context
						=>
					{
						// When resolved through the ServiceProviderScopeResolutionRoot which adds this parameter, the
						// correct _scoped_ IServiceProvider is used. Fall back to root IServiceProvider when not created
						// through a NinjectServiceProvider (some tests do this to prove a point)
						var scopeProvider = context.GetServiceProviderScopeParameter()?.SourceServiceProvider ??
						                    context.Kernel.Get<IServiceProvider>();
						return descriptor.KeyedImplementationFactory(scopeProvider, descriptor.ServiceKey) as T;
					}), descriptor.Lifetime);
				}
				else
				{
					// use ToMethod here as ToConstant has the wrong return type.
					result = bindingToSyntax.ToMethod(context => descriptor.KeyedImplementationInstance as T)
						.InSingletonScope();
				}

				result.WithMetadata("ServiceKey", descriptor.ServiceKey);
			}
			else
			{
#endif
				if (descriptor.ImplementationType != null)
				{
					result = ConfigureLifecycle(bindingToSyntax.To(descriptor.ImplementationType), descriptor.Lifetime);
				}
				else if (descriptor.ImplementationFactory != null)
				{
					result = ConfigureLifecycle(bindingToSyntax.ToMethod(context
						=>
					{
						// When resolved through the ServiceProviderScopeResolutionRoot which adds this parameter, the
						// correct _scoped_ IServiceProvider is used. Fall back to root IServiceProvider when not created
						// through a NinjectServiceProvider (some tests do this to prove a point)
						var scopeProvider = context.GetServiceProviderScopeParameter()?.SourceServiceProvider ??
						                    context.Kernel.Get<IServiceProvider>();
						return descriptor.ImplementationFactory(scopeProvider) as T;
					}), descriptor.Lifetime);
				}
				else
				{
					// use ToMethod here as ToConstant has the wrong return type.
					result = bindingToSyntax.ToMethod(context => descriptor.ImplementationInstance as T)
						.InSingletonScope();
				}
#if NET8_0_OR_GREATER
			}
#endif

			return result
				.WithMetadata(nameof(ServiceDescriptor), descriptor)
				.WithMetadata(nameof(BindingIndex), bindingIndex.Next(descriptor.ServiceType));
		}

		private IBindingNamedWithOrOnSyntax<T> ConfigureLifecycle<T>(
			IBindingInSyntax<T> bindingInSyntax,
			ServiceLifetime lifecycleKind)
		{
			switch (lifecycleKind)
			{
				case ServiceLifetime.Singleton:
					// Microsoft.Extensions.DependencyInjection expects its singletons to be disposed when the root service scope
					// and/or the root IServiceProvider is disposed.
					return bindingInSyntax.InScope(context => {
						return (context.Kernel as AspNetCoreKernel).RootScope;
					});
				case ServiceLifetime.Scoped:
					return bindingInSyntax.InRequestScope();
				case ServiceLifetime.Transient:
					// Microsoft.Extensions.DependencyInjection expects transient services to be disposed when the IServiceScope
					// in which they were created is disposed. See the compliance tests for more details.
					return bindingInSyntax.InScope(context => {
						var scope = context.GetServiceProviderScopeParameter();
						return scope?.DeriveTransientScope();
					});
				default:
					throw new NotSupportedException();
			}
		}
	}
}
