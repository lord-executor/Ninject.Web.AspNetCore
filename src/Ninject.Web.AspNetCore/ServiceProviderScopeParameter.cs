using Ninject.Infrastructure.Disposal;
using Ninject.Parameters;

namespace Ninject.Web.AspNetCore
{
	public class ServiceProviderScopeParameter : Parameter
	{
		public ServiceProviderScopeParameter(DisposableObject scope)
			: base(nameof(ServiceProviderScopeParameter), scope, true) { }
	}
}
