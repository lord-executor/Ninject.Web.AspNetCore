using Ninject.Modules;

namespace Ninject.Web.AspNetCore.Hosting
{
	/// <summary>
	/// This redirects the resolution of <see cref="AspNetCoreHostConfiguration"/> in <see cref="AspNetCoreHost"/> to <see cref="AspNetCoreHostHttpSysConfiguration"/>
	/// which is bound by the <see cref="Ninject.Web.Common.SelfHost.NinjectSelfHostBootstrapper"/> to constant.
	/// </summary>
	public class AspNetCoreHostingHttpSysModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<AspNetCoreHostConfiguration>().ToMethod(x => x.Kernel.Get<AspNetCoreHostHttpSysConfiguration>());
		}
	}
}
