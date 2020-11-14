using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Ninject.Web.AspNetCore.Hosting
{
	/// <summary>
	/// See Microsoft.AspNetCore.ForwardedHeadersStartupFilter
	/// </summary>
	public class ForwardedHeadersStartupFilter : IStartupFilter
	{
		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
		{
			return app =>
			{
				app.UseForwardedHeaders();
				next(app);
			};
		}
	}
}
