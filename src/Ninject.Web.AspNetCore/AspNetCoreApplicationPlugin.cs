using Ninject.Activation;
using Ninject.Components;
using Ninject.Web.Common;

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
			// returns the currently active request scope. Used when binding with scope InRequestScope.
			return RequestScope.Current;
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
