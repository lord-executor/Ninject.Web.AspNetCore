using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Activation;
using Ninject.Components;
using Ninject.Infrastructure;
using Ninject.Planning.Bindings;
using Ninject.Planning.Bindings.Resolvers;
using Ninject.Web.AspNetCore.RequestActivation;

namespace Ninject.Web.AspNetCore.Planning
{
#if NET8_0_OR_GREATER	
	/// <summary>
	/// This class is used to handle keyed services registrations with service key equal to
	/// KeyedService.AnyKey.
	/// If such a binding is resolved unique with a key different to KeyedService.AnyKey,
	/// we need to dynamically add a new matching binding with this service key.
	/// The missing binding resolver adds a binding with the metadata for this service key, so that
	/// it can be resolved in this resolution and in potential next resolutions as well. 
	/// </summary>
	public class KeyedServiceAnyKeyResolver : NinjectComponent, IMissingBindingResolver
	{
		public IEnumerable<IBinding> Resolve(Multimap<Type, IBinding> bindings, IRequest request)
		{
			// we resolve here request with a specific service key, but only having a binding with anykey.
			// this ensures that we e.g. can have a singleton binding with anykey, but instantiate one singleton
			// per servicekey.
			var keyedRequest = request as KeyedRequest;
			if (keyedRequest != null && keyedRequest.ServiceKey != null &&  keyedRequest.ServiceKey != KeyedService.AnyKey 
			    && keyedRequest.IsUnique)
			{
				var service = request.Service;
				var matchingBindings = bindings.Where(x => x.Key == service);
				if (!matchingBindings.Any())
				{
					return Array.Empty<IBinding>();
				}

				IBinding matchingAnyBinding = null;
				foreach (var bindingGroup in matchingBindings)
				{
					foreach (var binding in bindingGroup.Value)
					{
						if (binding.Metadata.HasServiceKeyMetadata() && Object.Equals(binding.Metadata.GetServiceKey(), KeyedService.AnyKey)
						    && (binding.Metadata.Get<BindingIndex.Item>(nameof(BindingIndex))?.IsLatest ?? true)
						    )
						{
							matchingAnyBinding = binding;
							break;
						}
					}
				}

				if (matchingAnyBinding == null)
				{
					return Array.Empty<IBinding>();
				}

				var resultBinding = new Binding(service)
				{
					IsImplicit = true,
					ProviderCallback = matchingAnyBinding.ProviderCallback,
					ScopeCallback = matchingAnyBinding.ScopeCallback,
					Target = matchingAnyBinding.Target
				};
				var bindingIndex = new BindingIndex();
				resultBinding.Metadata.Set(nameof(BindingIndex), bindingIndex.Next(service, keyedRequest.ServiceKey));
				resultBinding.Metadata.Set(nameof(ServiceKey), new ServiceKey(keyedRequest.ServiceKey));
				resultBinding.Metadata.Set(nameof(ServiceDescriptor), matchingAnyBinding.Metadata.Get<IDescriptorAdapter>(nameof(ServiceDescriptor)));

				return new Binding[1]
				{
					resultBinding
				};
			}

			return Array.Empty<IBinding>();
		}
	}
#endif
}