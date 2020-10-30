using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Ninject.Web.AspNetCore.Hosting
{
	/// <summary>
	/// See Microsoft.AspNetCore.HostFilteringStartupFilter.HostFilteringStartupFilter
	/// </summary>
	public class HostFilteringStartupFilter : IStartupFilter
	{
		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
		{
			return (IApplicationBuilder app) =>
			{
				app.UseHostFiltering();
				next(app);
			};
		}
	}
}
