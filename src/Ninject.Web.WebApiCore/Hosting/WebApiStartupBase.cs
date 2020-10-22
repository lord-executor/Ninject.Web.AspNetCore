using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.WebApiCore.Hosting
{
    /// <summary>
    /// NInject customized startup class similar to Microsoft.AspNetCore.Hosting.StartupBase of TBuilder.
    /// </summary>
    public abstract class WebApiStartupBase : IStartup
	{
		private readonly IServiceProviderFactory<NInjectServiceProviderBuilder> _providerFactory;

		protected WebApiStartupBase(IServiceProviderFactory<NInjectServiceProviderBuilder> providerFactory)
		{
			_providerFactory = providerFactory;
		}

		public abstract void Configure(IApplicationBuilder app);

		IServiceProvider IStartup.ConfigureServices(IServiceCollection services)
		{
			var mvcBuilder = services.AddMvc()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			ConfigureMvcOptions(mvcBuilder);
			services.AddRouting();
			ConfigureServices(services); // allow to customize the services before conversion to NInject bindings happen.
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

