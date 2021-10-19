using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ninject.Web.AspNetCore.Hosting
{
	/// <summary>
	/// Creates the <see cref="RequestScope"/> at the start of the request and destroys it at the end.
	/// We do NOT implement <see cref="IMiddleware"/> here deliberately so that only one instance of the
	/// middleware will be created. Since the middleware does not have any actual state or dependencies
	/// this seems to be the much better option for performance.
	/// See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write?view=aspnetcore-5.0#per-request-middleware-dependencies
	/// </summary>
	public class RequestScopeMiddleware
	{
		private readonly RequestDelegate _next;

		public RequestScopeMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			using (new RequestScope())
			{
				await _next.Invoke(context);
			}
		}
	}
}
