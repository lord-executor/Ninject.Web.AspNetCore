using System;
using System.Collections.Generic;

namespace Ninject.Web.AspNetCore
{
	public class BindingIndex
	{
		private readonly IDictionary<Type, Item> _bindingIndexMap = new Dictionary<Type, Item>();

		public int Count { get; private set; }

		public BindingIndex()
		{
		}

		public Item Next(Type serviceType)
		{
			
			_bindingIndexMap.TryGetValue(serviceType, out var previous);

			var next = new Item(this, serviceType, Count++, previous?.TypeIndex + 1 ?? 0);
			_bindingIndexMap[serviceType] = next;

			return next;
		}

		private bool IsLatest(Type serviceType, Item item)
		{
			return _bindingIndexMap[serviceType] == item;
		}

		public class Item
		{
			private readonly BindingIndex _root;
			private readonly Type _serviceType;

			public int TotalIndex { get; }
			public int TypeIndex { get; }

			public bool IsLatest => _root.IsLatest(_serviceType, this);
			public int Precedence => _root.Count - TotalIndex;

			public Item(BindingIndex root, Type serviceType, int totalIndex, int typeIndex)
			{
				_root = root;
				_serviceType = serviceType;
				TotalIndex = totalIndex;
				TypeIndex = typeIndex;
			}
		}
	}
}
