using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.WebApiCore
{
	public class FixServicesForPublicatonAdapter : IPopulateAdapter
	{
		public void AdaptAfterPopulate(IKernel kernel)
		{
			// AddControllersAsServices would replace the IControllerActivator by ServiceBasedControllerActivator, which is needed that NInject can instantiate controllers
			// as we don't need the autobinding of the controllers, we should not call it during startup. Instead we will replace in NInject the ControllerActivator.
			kernel.Rebind<IControllerActivator>().To<ServiceBasedControllerActivator>().InTransientScope();
		}

		public bool AdaptDescriptor(IKernel kernel, ServiceDescriptor serviceDescriptor)
		{
			return false;
		}
	}
}
