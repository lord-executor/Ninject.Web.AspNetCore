using Microsoft.AspNetCore.Hosting;
using System;

namespace Ninject.Web.AspNetCore.Hosting
{
	public interface IAspNetCoreHostConfiguration
	{
		void ConfigureWebHostBuilder(Func<IWebHostBuilder> webHostBuilderFactory);

		void ConfigureStartupType(Type startupType);

		void ConfigureHostingModel(Action<IWebHostBuilder> configureAction);
	}
}
