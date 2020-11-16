using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Web.AspNetCore.Hosting;
using System;

namespace Ninject.Web.AspNetCore
{
	public class FixServicesForPublicatonAdapter : IPopulateAdapter
	{
		private AspNetCoreHostConfiguration _configuration;

		public FixServicesForPublicatonAdapter(AspNetCoreHostConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void AdaptAfterPopulate(IKernel kernel)
		{
			var controllerActivatorType = (_configuration.CustomControllerActivator != null) ? _configuration.CustomControllerActivator : typeof(ServiceBasedControllerActivator);
			
			// AddControllersAsServices would replace the IControllerActivator by ServiceBasedControllerActivator, which is needed that Ninject can instantiate controllers
			// as we don't need the autobinding of the controllers, we should not call it during startup. Instead we will replace in Ninject the ControllerActivator.
			kernel.Rebind<IControllerActivator>().To(controllerActivatorType).InTransientScope();
		}

		public bool AdaptDescriptor(IKernel kernel, ServiceDescriptor serviceDescriptor)
		{
			return false;
		}
	}
}
