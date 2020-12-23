using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ninject.Web.AspNetCore.Hosting
{
	/// <summary>
	/// Creates the <see cref="RequestScope"/> at the start of the request and destroys it at the end.
	/// </summary>
	public class RequestScopeMiddleware : IMiddleware
	{
		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			using (new RequestScope())
			{
				await next.Invoke(context);
			}
		}
	}
}
