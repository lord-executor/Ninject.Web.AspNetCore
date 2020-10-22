using Ninject.Modules;
using Ninject.Web.WebApiCore.Hosting;

namespace Ninject.Web.WebApiCore.Hosting
{
	public class WebApiHostingHttpSysModule : NinjectModule
	{
		public override void Load()
		{
			// this redirects the resolution of WebApiHostConfiguration in WebApiHost to WebApiHostConfigurationHttpSys, which is bound by the NinjectSelfHostWrapper to constant.
			Kernel.Bind<WebApiHostConfiguration>().ToMethod(x => x.Kernel.Get<WebApiHostHttpSysConfiguration>());
		}
	}
}
