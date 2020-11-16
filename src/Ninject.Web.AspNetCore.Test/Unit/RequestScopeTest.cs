using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ninject.Activation;
using Ninject.Web.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class RequestScopeTest : TestKernelContext
	{
		
		[Fact]
		public void ApplicationPluginRegistered()
		{
			var kernel = CreateKernel(new ServiceCollection());

			var appPlugin = kernel.Components.GetAll<INinjectHttpApplicationPlugin>();
			appPlugin.Should().Contain(x => x is AspNetCoreApplicationPlugin);
		}

		[Fact]
		public void RequestScopeAvailableWhenRequestRunning()
		{
			var kernel = CreateKernel(new ServiceCollection());
			var contextMock = new Mock<IContext>();
			
			using (var aspNetScope = new NinjectServiceScope(kernel))
			{				
				var requestScope = kernel.Components.GetAll<INinjectHttpApplicationPlugin>().Select(c => c.GetRequestScope(contextMock.Object)).FirstOrDefault(s => s != null);
				requestScope.Should().NotBeNull().And.BeOfType(typeof(RequestScope));
			}
		}

		[Fact]
		public void RequestScopeNotAvailableWhenNoRequestRunning()
		{
			var kernel = CreateKernel(new ServiceCollection());
			var contextMock = new Mock<IContext>();

			var requestScope = kernel.Components.GetAll<INinjectHttpApplicationPlugin>().Select(c => c.GetRequestScope(contextMock.Object)).FirstOrDefault(s => s != null);
			requestScope.Should().BeNull();
		}
	}
}
