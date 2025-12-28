using System;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Planning.Bindings;

namespace Ninject.Web.AspNetCore.Planning
{
	/// <summary>
	/// Extensions to handle ServiceKey.
	/// Only really relevant for >= .NET 8.0, as only there Microsoft DI supports keyed services.
	/// </summary>
	public static class KeyedServicesMetaDataExtensions
	{

#if NET8_0_OR_GREATER
		internal static bool DoesMetadataMatchServiceKey(this IBindingMetadata metadata, object serviceKey, bool isUniqueRequest)
		{
			if (serviceKey == KeyedService.AnyKey)
			{
				// if the service is registered with KeyedService.AnyKey, it must not be returned when querying with AnyKey
				// see CombinationalRegistration compliancetest
				return HasServiceKeyMetadata(metadata) && !Object.Equals(metadata.GetServiceKey(), KeyedService.AnyKey);
			}
			return Object.Equals(metadata.GetServiceKey(), serviceKey)
		       // if we query with a key different to KeyedService.AnyKey but registired with AnyKey, we have to return it as well
		       // see ResolveKeyedServiceSingletonInstanceWithAnyKey compliancetest. But only if we resolve a unique instance
				|| (isUniqueRequest && Object.Equals(metadata.GetServiceKey(), KeyedService.AnyKey));
		}

		internal static object GetServiceKey(this IBindingMetadata metadata)
		{
			return metadata.Get<ServiceKey>(nameof(ServiceKey))?.Key;
		}
#endif
		internal static bool HasServiceKeyMetadata(this IBindingMetadata metadata)
		{
			return metadata.Has(nameof(ServiceKey));
		}
	}
}