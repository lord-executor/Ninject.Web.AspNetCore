using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore.Hosting
{
	public class EmptyStartup : AspNetCoreStartupBase
	{
		public EmptyStartup(IServiceProviderFactory<NinjectServiceProviderBuilder> providerFactory) : base(providerFactory)
		{
		}

		public override void Configure(IApplicationBuilder app)
		{
		}
	}
}
