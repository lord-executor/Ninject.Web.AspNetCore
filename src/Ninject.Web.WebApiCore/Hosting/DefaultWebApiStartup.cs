using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.WebApiCore.Hosting
{
	public class DefaultWebApiStartup : WebApiStartupBase
	{
		public DefaultWebApiStartup(IServiceProviderFactory<NInjectServiceProviderBuilder> providerFactory) : base(providerFactory)
		{
		}

		public override void Configure(IApplicationBuilder app)
		{
		}
	}
}
