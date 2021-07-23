using Ninject.Infrastructure.Disposal;
using Ninject.Parameters;
using System.Collections.Generic;

namespace Ninject.Web.AspNetCore
{
	public class ServiceProviderScopeParameter : Parameter
	{
		private readonly DisposableObject _scope;
		private readonly IList<TransientScope> _children = new List<TransientScope>();

		public ServiceProviderScopeParameter(DisposableObject scope)
			: base(nameof(ServiceProviderScopeParameter), scope, true)
		{
			_scope = scope;
			_scope.Disposed += (_, _) =>
			{
				foreach (var child in _children)
				{
					child.Dispose();
				}
			};
		}

		public DisposableObject DeriveTransientScope()
		{
			var child = new TransientScope();
			_children.Add(child);
			return child;
		}

		private class TransientScope : DisposableObject
		{
		}
	}
}
