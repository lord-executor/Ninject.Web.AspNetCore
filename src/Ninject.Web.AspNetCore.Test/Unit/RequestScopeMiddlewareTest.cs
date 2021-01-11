using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Ninject.Web.AspNetCore.Hosting;
using System.Threading.Tasks;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class RequestScopeMiddlewareTest
	{
		[Fact]
		public void Middleware_WhenInvoked_CreatesAndDestroysRequestScope()
		{
			var middleware = new RequestScopeMiddleware();
			var httpContextMock = new Mock<HttpContext>();

			RequestScope.Current.Should().BeNull();
			middleware.InvokeAsync(httpContextMock.Object, _ =>
			{
				// make sure that there is actually some asynchronicity in the pipeline
				return Task.Delay(200).ContinueWith(_ =>
				{
					RequestScope.Current.Should().NotBeNull();
				});
			}).Wait();
			RequestScope.Current.Should().BeNull();
		}
	}
}
