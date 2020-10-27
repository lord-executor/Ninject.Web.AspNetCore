using Microsoft.Extensions.DependencyInjection;
using Ninject.Modules;
using Ninject.Web.Common;

namespace Ninject.Web.AspNetCore
{
	/// <summary>
	/// Defines the bindings for the ASP.NET Core extension
	/// </summary>
	public class AspNetCoreModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Components.Add<INinjectHttpApplicationPlugin, AspNetCoreApplicationPlugin>(); // provides the scope object for InRequestScope bindings
			Kernel.Bind<IServiceScopeFactory>().To<NInjectServiceScopeFactory>().InTransientScope().WithConstructorArgument("kernel", Kernel);
			Kernel.Bind<IPopulateAdapter>().To<FixDoubleBindingAdapter>();
			Kernel.Bind<IPopulateAdapter>().To<FixServicesForPublicatonAdapter>();
		}
	}
}
