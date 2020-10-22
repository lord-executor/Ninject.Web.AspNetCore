using Microsoft.AspNetCore.Mvc.Controllers;
using Ninject.Activation;
using Ninject.Components;
using Ninject.Web.Common;

namespace Ninject.Web.WebApiCore
{
	public class WebApiApplicationPlugin : NinjectComponent, INinjectHttpApplicationPlugin
	{
		private readonly IKernel _kernel;

		public WebApiApplicationPlugin(IKernel kernel)
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
