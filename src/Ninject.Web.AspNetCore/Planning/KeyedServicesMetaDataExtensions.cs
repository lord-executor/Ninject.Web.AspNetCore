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
		internal static bool DoesMetadataMatchServiceKey(this IBindingMetadata metadata, object serviceKey)
		{
			if (serviceKey == KeyedService.AnyKey)
			{
				return HasServiceKeyMetadata(metadata);
			}
			return Object.Equals(metadata.GetServiceKey(), serviceKey);
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