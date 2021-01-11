using Ninject.Activation;
using Ninject.Components;
using Ninject.Web.Common;
using System;
using System.Linq;

namespace Ninject.Web.AspNetCore
{
	public class AspNetCoreApplicationPlugin : NinjectComponent, INinjectHttpApplicationPlugin
	{
		private readonly IKernel _kernel;
		private readonly MissingRequestScopeBehaviorType _scopeBehavior;

		public AspNetCoreApplicationPlugin(IKernel kernel)
		{
			_kernel = kernel;
			_scopeBehavior = _kernel.Settings.GetMissingRequestScopeBehavior();
		}

		public object GetRequestScope(IContext context)
		{
			// when being instantiated through the ServiceProviderScopeResolutionRoot, the parameter for explicit nested scopes
			// created through IServiceScopeFactory.CreateScope has precedence in order to preserve the behavior that is expected
			// from IServiceProvider with scoped services.
			var scope = context.Parameters.OfType<ServiceProviderScopeParameter>().SingleOrDefault()?.GetValue(context, null);
			// returns the currently active request scope. Used when binding with scope InRequestScope.
			return scope ?? RequestScope.Current ?? GetFallbackScope();
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

		private object GetFallbackScope()
		{
			switch (_scopeBehavior)
			{
				case MissingRequestScopeBehaviorType.Throw:
					throw new ActivationException("Trying to activate a service InRequestScope without a request scope present");

				case MissingRequestScopeBehaviorType.UseKernel:
					return _kernel;

				case MissingRequestScopeBehaviorType.UseTransient:
					return null;
			}

			throw new ArgumentException("Unknown MissingRequestScopeBehaviorType", nameof(_scopeBehavior));
		}
	}
}
