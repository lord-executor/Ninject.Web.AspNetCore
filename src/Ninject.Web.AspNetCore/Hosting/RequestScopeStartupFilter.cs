using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Ninject.Web.AspNetCore.Hosting
{
	public class RequestScopeStartupFilter : IStartupFilter
	{
		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
		{
			return (IApplicationBuilder app) =>
			{
				app.UseMiddleware<RequestScopeMiddleware>();
				next(app);
			};
		}
	}
}
