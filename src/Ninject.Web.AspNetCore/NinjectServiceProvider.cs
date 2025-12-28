using Microsoft.Extensions.DependencyInjection;
using Ninject.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject.Planning.Bindings;
using Ninject.Web.AspNetCore.Planning;

namespace Ninject.Web.AspNetCore
{
	/// <summary>
	/// We wrap the <see cref="IResolutionRoot" /> here to explicitly implement both the <see cref="IServiceProvider" /> and
	/// <see cref="ISupportRequiredService" /> to give us more control.
	/// 
	/// Note: ASP.NET Core wants to use a method from ISupportRequiredService to resolve a non-optional service.
	/// Although it's implemented on Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions in a generic way
	/// we implement it here to have the nicer exceptions from Ninject so that it's possible to distinguish the "not registered at all"
	/// vs the "ambigious matches found" cases.
	/// 
	/// Also, even though <see cref="IServiceProvider"/> does NOT implement <see cref="IDisposable"/>,
	/// Microsoft.Extensions.DependencyInjection assumes that the service provider does implement it and disposes all of its
	/// instances that are associated with the root scope. This is why we implement <see cref="IDisposable"/> and this is why
	/// we pass an <see cref="IServiceScope"/> constructor argument when creating the root service provider.
	/// </summary>
	public class NinjectServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable
#if NET8_0_OR_GREATER
	, IKeyedServiceProvider
#endif
	{
		private static readonly MethodInfo EnumerableCastMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast));
		private readonly IResolutionRoot _resolutionRoot;
		private readonly IServiceScope _scope;

		public NinjectServiceProvider(IResolutionRoot resolutionRoot, IServiceScope scope)
		{
			_resolutionRoot = resolutionRoot;
			_scope = scope;
		}

		public object GetRequiredService(Type serviceType)
		{
			if (!IsListType(serviceType, out var elementType))
			{
				return _resolutionRoot.Get(serviceType, metadata => !metadata.HasServiceKeyMetadata());
			}
			else
			{
				// Ninject is not evaluating metadata constraint when resolving a IEnumerable<T>, see KernelBase.UpdateRequest
				// Therefore, need to implement a workaround to not instantiate here bindings with servicekey
				return ConvertToTypedEnumerable(elementType,
					_resolutionRoot.GetAll(elementType, metadata => !metadata.HasServiceKeyMetadata()));
			}
		}

		public object GetService(Type serviceType)
		{
			object result;
			if (!IsListType(serviceType, out var elementType))
			{
				result = _resolutionRoot.TryGet(serviceType, metadata => !metadata.HasServiceKeyMetadata());
			}
			else
			{
				// Ninject is not evaluating metadata constraint when resolving a IEnumerable<T>, see KernelBase.UpdateRequest
				// Therefore, need to implement a workaround to not instantiate here bindings with servicekey
				result = ConvertToTypedEnumerable(elementType,
					_resolutionRoot.GetAll(elementType, metadata => !metadata.HasServiceKeyMetadata()));
			}

			return result;
		}

		public void Dispose()
		{
			_scope?.Dispose();
		}

#if NET8_0_OR_GREATER
		public object GetKeyedService(Type serviceType, object serviceKey)
		{
			if (serviceKey == null)
			{
				// serviceKey = null means unkeyed
				return GetService(serviceType);
			}

			object result;
			if (!IsListType(serviceType, out var elementType))
			{
				EnsureNotAnyKey(serviceKey, serviceType);
				result = _resolutionRoot.TryGet(serviceType,
					metadata => metadata.DoesMetadataMatchServiceKey(serviceKey, true));
			}
			else
			{
				// Ninject is not evaluating metadata constraint when resolving a IEnumerable<T>, see KernelBase.UpdateRequest
				// Therefore, need to implement a workaround to not instantiate here bindings with a different servicekey value
				result = ConvertToTypedEnumerable(elementType,
					_resolutionRoot.GetAll(elementType, metadata => metadata.DoesMetadataMatchServiceKey(serviceKey, false)));
			}

			return result;
		}

		public object GetRequiredKeyedService(Type serviceType, object serviceKey)
		{
			if (serviceKey == null)
			{
				// serviceKey = null means unkeyed
				return GetRequiredService(serviceType);
			}

			if (!IsListType(serviceType, out var elementType))
			{
				EnsureNotAnyKey(serviceKey, serviceType);
				return _resolutionRoot.Get(serviceType, metadata => metadata.DoesMetadataMatchServiceKey(serviceKey, true));
			}
			else
			{
				// Ninject is not evaluating metadata constraint when resolving a IEnumerable<T>, see KernelBase.UpdateRequest
				// Therefore, need to implement a workaround to not instantiate here bindings with a different servicekey value
				return ConvertToTypedEnumerable(elementType,
					_resolutionRoot.GetAll(elementType, metadata => metadata.DoesMetadataMatchServiceKey(serviceKey, false)));
			}
		}

		private void EnsureNotAnyKey(object serviceKey, Type serviceType)
		{
			if (serviceKey == KeyedService.AnyKey)
			{
				throw new InvalidOperationException($"Not allowed to resolve a service {serviceType} with the KeyedService.AnyKey. " +
				                                    $"That's only supported when resolving collections of services.");
			}
		}

#endif

		/// <summary>
		/// This method extracts the elementtype in the same way as Ninject does
		/// in KernelBase.Resolve
		/// </summary>
		private static bool IsListType(Type type, out Type elementType)
		{
			if (type.IsArray)
			{
				elementType = type.GetElementType();
				return true;
			}

			if (type.IsGenericType)
			{
				Type genericTypeDefinition = type.GetGenericTypeDefinition();
				if (genericTypeDefinition == typeof(List<>) || genericTypeDefinition == typeof(IList<>) ||
				    genericTypeDefinition == typeof(ICollection<>))
				{
					elementType = type.GenericTypeArguments[0];
					return true;
				}

				if (genericTypeDefinition == typeof(IEnumerable<>))
				{
					elementType = type.GenericTypeArguments[0];
					return true;
				}
			}

			elementType = null;
			return false;
		}
		
		private static object ConvertToTypedEnumerable(Type elementType, IEnumerable<object> objectList)
		{
			var castMethod = EnumerableCastMethod.MakeGenericMethod(elementType);
			var result = (IEnumerable)castMethod.Invoke(null, new object[] { objectList });
			return result;
		}
		
	}
}
