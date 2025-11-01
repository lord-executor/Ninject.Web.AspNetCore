using AwesomeAssertions;
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
		public async Task Middleware_WhenInvoked_CreatesAndDestroysRequestScope()
		{
			var callCount = 0;
			var middleware = new RequestScopeMiddleware(_ =>
			{
				// make sure that there is actually some asynchronicity in the pipeline
				return Task.Delay(200).ContinueWith(_ =>
				{
					callCount++;
					RequestScope.Current.Should().NotBeNull();
				});
			});
			var httpContextMock = new Mock<HttpContext>();

			RequestScope.Current.Should().BeNull();
			await middleware.InvokeAsync(httpContextMock.Object);
			callCount.Should().Be(1);
			RequestScope.Current.Should().BeNull();
		}
	}
}
