using Microsoft.Extensions.DependencyInjection;
using Ninject.Components;
using Ninject.Infrastructure;
using Ninject.Planning.Bindings;
using Ninject.Planning.Bindings.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ninject.Web.AspNetCore.Components
{
	/// <summary>
	/// This is an improved version of the <see cref="OpenGenericBindingResolver"/> which is not capable of considering constrained generics
	/// since the binding does not contain the necessary metadata to determine the binding target type. This is solved here by adding the
	/// original service descriptor from Microsoft.Extensions.DependencyInjection to the binding metadata with the <see cref="ServiceCollectionAdapter"/>.
	/// With that we can filter the bindings to only contain those that are compatible with the target type's generic type constraints.
	/// </summary>
	public class ConstrainedGenericBindingResolver : NinjectComponent, IBindingResolver
	{
		public IEnumerable<IBinding> Resolve(Multimap<Type, IBinding> bindings, Type service)
		{
			if (!service.IsGenericType || service.IsGenericTypeDefinition || !bindings.ContainsKey(service.GetGenericTypeDefinition()))
			{
				return Enumerable.Empty<IBinding>();
			}

			// We don't need to do any caching here since the resolved bindings are automatically placed in the binding cache of Ninject so
			// that the next request for the same service type doesn't need to resolve this again.
			return bindings[service.GetGenericTypeDefinition()].Where(binding => {
				// If the binding has a ServiceDescriptor in its metadata, then we 
				if (binding.Target == BindingTarget.Type && binding.Metadata.Has(nameof(ServiceDescriptor)))
				{
					return SatisfiesGenericTypeConstraints(service, binding.Metadata.Get<ServiceDescriptor>(nameof(ServiceDescriptor)).ImplementationType);
				}

				// ... otherwise we default to the OpenGenericBindingResolver which returns _all_ the bindings without regard for their generic constraints
				return true;
			});
		}

		/// <summary>
		/// This method checks if the <paramref name="boundType"/> which is supposed to be a generic type definition can accept the <paramref name="requestedType"/>s
		/// generic arguments. If such a constructed generic type is valid and can be assigned to the <paramref name="requestedType"/>, then the constrained generic
		/// binding resolver considers the corresponding binding to be a match for the requested service.
		/// </summary>
		/// <returns><c>true</c> if and only if the bound open generic type can be used to create an instance compatible with the requested type</returns>
		/// <exception cref="ArgumentException">Thrown if the bound type is not a generic type definition</exception>
		public bool SatisfiesGenericTypeConstraints(Type requestedType, Type boundType)
		{
			if (!boundType.IsGenericTypeDefinition)
			{
				throw new ArgumentException("Bound type must be a generic type definition", nameof(boundType));
			}

			try
			{
				var genericArguments = requestedType.GetGenericArguments();
				var constructedType = boundType.MakeGenericType(genericArguments);

				return constructedType.IsAssignableTo(requestedType);
			}
			catch (ArgumentException)
			{
				return false;
			}
		}
	}
}
