using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore.Hosting
{
    /// <summary>
    /// Ninject customized startup class similar to Microsoft.AspNetCore.Hosting.StartupBase of TBuilder.
    /// </summary>
    public abstract class AspNetCoreStartupBase : IStartup
	{
		private readonly IServiceProviderFactory<NinjectServiceProviderBuilder> _providerFactory;

		protected AspNetCoreStartupBase(IServiceProviderFactory<NinjectServiceProviderBuilder> providerFactory)
		{
			_providerFactory = providerFactory;
		}

		public abstract void Configure(IApplicationBuilder app);

		IServiceProvider IStartup.ConfigureServices(IServiceCollection services)
		{
			var mvcBuilder = services.AddMvc();

			ConfigureMvcOptions(mvcBuilder);
			ConfigureServices(services); // allow to customize the services before conversion to Ninject bindings happen.
			return _providerFactory.CreateBuilder(services).Build();
		}

		public virtual void ConfigureServices(IServiceCollection services)
		{
		}

		public virtual void ConfigureMvcOptions(IMvcBuilder builder)
		{
		}

	}



}

