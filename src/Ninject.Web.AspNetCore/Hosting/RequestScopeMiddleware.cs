using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ninject.Web.AspNetCore.Hosting
{
	/// <summary>
	/// Creates the <see cref="RequestScope"/> at the start of the request and destroys it at the end.
	/// </summary>
	public class RequestScopeMiddleware : IMiddleware
	{
		public Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			var scope = new RequestScope();
			return next.Invoke(context).ContinueWith(_ => scope.Dispose());
		}
	}
}
