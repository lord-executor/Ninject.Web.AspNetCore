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

			foreach (var descriptor in serviceCollection)
			{
				if (UsedCustomBinding(kernel, adapters, descriptor))
				{
					continue;
				}

				ConfigureImplementationAndLifecycle(kernel.Bind(descriptor.ServiceType), descriptor);
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
			ServiceDescriptor descriptor) where T : class
		{
			if (!_bindingIndexMap.ContainsKey(descriptor.ServiceType))
			{
				_bindingIndexMap[descriptor.ServiceType] = new BindingIndex();
			}
			var bindingIndex = _bindingIndexMap[descriptor.ServiceType];

			IBindingNamedWithOrOnSyntax<T> result;
			if (descriptor.ImplementationType != null)
			{
				result = ConfigureLifecycle(bindingToSyntax.To(descriptor.ImplementationType), descriptor.Lifetime);
				result.WithMetadata("BoundType", descriptor.ImplementationType);
			}
			else if (descriptor.ImplementationFactory != null)
			{

				result = ConfigureLifecycle(bindingToSyntax.ToMethod(context
					=>
				{
					var provider = context.Kernel.Get<IServiceProvider>();
					return descriptor.ImplementationFactory(provider) as T;
				}), descriptor.Lifetime);
			}
			else
			{
				// use ToMethod here as ToConstant has the wrong return type.
				result = bindingToSyntax.ToMethod(context => descriptor.ImplementationInstance as T).InSingletonScope();
			}

			return result
				.WithMetadata(nameof(ServiceDescriptor), descriptor)
				.WithMetadata(nameof(BindingIndex), bindingIndex.Next());
		}

		private IBindingNamedWithOrOnSyntax<T> ConfigureLifecycle<T>(
			IBindingInSyntax<T> bindingInSyntax,
			ServiceLifetime lifecycleKind)
		{
			switch (lifecycleKind)
			{
				case ServiceLifetime.Singleton:
					return bindingInSyntax.InSingletonScope();
				case ServiceLifetime.Scoped:
					return bindingInSyntax.InRequestScope();
				case ServiceLifetime.Transient:
					//return bindingInSyntax.InTransientScope();
					// Microsoft.Extensions.DependencyInjection expects transient services to be disposed
					// See the compliance tests for more details.
					return bindingInSyntax.InScope(context => {
						var scope = context.Parameters.OfType<ServiceProviderScopeParameter>().SingleOrDefault();
						return scope?.DeriveTransientScope();
					});
				default:
					throw new NotSupportedException();
			}
		}
	}
}
