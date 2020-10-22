using Ninject.Modules;
using Ninject.Web.Common.SelfHost;

namespace Ninject.Web.WebApiCore.Hosting
{
	public class WebApiHostingModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<INinjectSelfHost>().To<WebApiHost>();
		}
	}
}
