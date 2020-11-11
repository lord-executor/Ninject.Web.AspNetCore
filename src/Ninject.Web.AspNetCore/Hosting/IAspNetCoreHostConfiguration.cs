using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading;

namespace Ninject.Web.AspNetCore.Hosting
{
	public interface IAspNetCoreHostConfiguration
	{
		void ConfigureWebHostBuilder(Func<IWebHostBuilder> webHostBuilderFactory);

		void ConfigureStartupType(Type startupType);

		void ConfigureHostingModel(Action<IWebHostBuilder> configureAction);

		void ConfigureStartupBehavior(bool blockOnStart, CancellationToken cancellationToken);
	}
}
