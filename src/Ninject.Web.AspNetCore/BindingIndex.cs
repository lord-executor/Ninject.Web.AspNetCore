using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore
{
	public class BindingIndex
	{

		/// <summary>
		/// Used as key for storing BindingIndex for unkeyed services.
		/// </summary>
		public sealed class UnkeyedIndexKey
		{
			private UnkeyedIndexKey()
			{
			}

			public static UnkeyedIndexKey Instance { get; } = new UnkeyedIndexKey();

			public override string ToString() => nameof(UnkeyedIndexKey);
		}

		private readonly Dictionary<ServiceTypeKey, Item> _bindingIndexMap = new Dictionary<ServiceTypeKey, Item>();

		public int Count { get; private set; }

		public BindingIndex()
		{
		}

		public Item Next(Type serviceType, object indexKey)
		{
			var serviceTypeKey = new ServiceTypeKey(serviceType, indexKey);
			_bindingIndexMap.TryGetValue(serviceTypeKey, out var previous);

			var next = new Item(this, serviceType, indexKey, Count++, previous?.TypeIndex + 1 ?? 0);
			_bindingIndexMap[serviceTypeKey] = next;

			return next;
		}

		private bool IsLatest(Type serviceType, object registeredIndexKey, Item item)
		{
			var match = _bindingIndexMap[new ServiceTypeKey(serviceType, registeredIndexKey)] == item;
			return match;
		}

		public class Item
		{
			private readonly BindingIndex _root;
			private readonly Type _serviceType;

			public int TotalIndex { get; }
			public int TypeIndex { get; }
			public object IndexKey { get; }
			
			public int Precedence => _root.Count - TotalIndex;

			public Item(BindingIndex root, Type serviceType, object indexKey, int totalIndex, int typeIndex)
			{
				_root = root;
				_serviceType = serviceType;
				TotalIndex = totalIndex;
				TypeIndex = typeIndex;
				IndexKey = indexKey;
			}
			
			public bool IsLatest => _root.IsLatest(_serviceType, IndexKey, this);
		}

		/// <summary>
		/// We have to to separate the precedence by servicekey.
		/// This ensures that a binding with a different servicekey
		/// can't override a binding with a non-matching servicekey
		/// </summary>
		public class ServiceTypeKey : IEquatable<ServiceTypeKey>
		{
			public Type ServiceType { get; }
			public object IndexKey { get; }

			public ServiceTypeKey(Type serviceType, object indexKey)
			{
				ServiceType = serviceType;
				IndexKey = indexKey;
			}

			public bool Equals(ServiceTypeKey other)
			{
				if (other is null) return false;
				if (ReferenceEquals(this, other)) return true;
				return Equals(ServiceType, other.ServiceType) && Equals(IndexKey, other.IndexKey);
			}

			public override bool Equals(object obj)
			{
				if (obj is null) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != GetType()) return false;
				return Equals((ServiceTypeKey)obj);
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(ServiceType, IndexKey);
			}
		}
	}
}
