using Microsoft.Extensions.DependencyInjection;
using Ninject.Modules;
using Ninject.Web.Common;
using Ninject.Web.Common.SelfHost;
using Ninject.Web.WebApiCore.Hosting;

namespace Ninject.Web.WebApiCore
{
	/// <summary>
	/// Defines the bindings for the webapi core extension
	/// </summary>
	public class WebApiModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Components.Add<INinjectHttpApplicationPlugin, WebApiApplicationPlugin>(); // provides the scope object for InRequestScope bindings
			Kernel.Bind<IServiceScopeFactory>().To<NInjectServiceScopeFactory>().InTransientScope().WithConstructorArgument("kernel", Kernel);
			Kernel.Bind<IPopulateAdapter>().To<FixDoubleBindingAdapter>();
			Kernel.Bind<IPopulateAdapter>().To<FixServicesForPublicatonAdapter>();
		}
	}
}
