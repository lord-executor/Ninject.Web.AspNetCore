using Ninject.Activation;
using Ninject.Components;
using Ninject.Web.Common;
using System.Linq;

namespace Ninject.Web.AspNetCore
{
	public class AspNetCoreApplicationPlugin : NinjectComponent, INinjectHttpApplicationPlugin
	{
		private readonly IKernel _kernel;

		public AspNetCoreApplicationPlugin(IKernel kernel)
		{
			_kernel = kernel;
		}

		public object GetRequestScope(IContext context)
		{
			// when being instantiated through the ServiceProviderScopeResolutionRoot, the parameter for explicit nested scopes
			// created through IServiceScopeFactory.CreateScope has precedence in order to preserve the behavior that is expected
			// from IServiceProvider with scoped services.
			var scope = context.Parameters.OfType<ServiceProviderScopeParameter>().SingleOrDefault()?.GetValue(context, null);
			// returns the currently active request scope. Used when binding with scope InRequestScope.
			return scope ?? RequestScope.Current;
		}

		// start is called after kernel is completely configured by the bootstrapper.
		public void Start()
		{
			// nothing to do
		}

		public void Stop()
		{
			// nothing to do
		}

	}
}
