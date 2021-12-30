using Ninject.Infrastructure.Disposal;
using Ninject.Parameters;
using System;
using System.Collections.Generic;

namespace Ninject.Web.AspNetCore
{
	public class ServiceProviderScopeParameter : Parameter
	{
		private readonly NinjectServiceScope _scope;
		private readonly IList<TransientScope> _children = new List<TransientScope>();

		public IServiceProvider SourceServiceProvider => _scope.ServiceProvider;

		public ServiceProviderScopeParameter(NinjectServiceScope scope)
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
