using Ninject.Modules;
using Ninject.Web.Common.SelfHost;

namespace Ninject.Web.AspNetCore.Hosting
{
	public class AspNetCoreHostingModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<INinjectSelfHost>().To<AspNetCoreHost>();
		}
	}
}
