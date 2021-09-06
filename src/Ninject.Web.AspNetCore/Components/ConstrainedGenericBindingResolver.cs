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

			// TODO: since reflection is pretty inefficient, we should probably cache this information
			return bindings[service.GetGenericTypeDefinition()].Where(binding => {
				if (binding.Target == BindingTarget.Type && binding.Metadata.Has(nameof(ServiceDescriptor)))
				{
					var boundType = binding.Metadata.Get<ServiceDescriptor>(nameof(ServiceDescriptor)).ImplementationType;
					var genericArguments = boundType.GetGenericArguments();
					var realArguments = service.GenericTypeArguments;
					for (var i = 0; i < genericArguments.Length; i++)
					{
						foreach (var constraint in genericArguments[i].GetGenericParameterConstraints())
						{
							if (!constraint.IsAssignableFrom(realArguments[i]))
							{
								return false;
							}
						}
					}
				}

				return true;
			});
		}
	}
}
