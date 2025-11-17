using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Web.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class AspNetCoreApplicationPluginTest : TestKernelContext
	{
		
		[Fact]
		public void ComponentsOfKernel_ContainsApplicationPlugin()
		{
			var kernel = CreateKernel(new ServiceCollection());

			var appPlugin = kernel.Components.GetAll<INinjectHttpApplicationPlugin>();
			appPlugin.Should().Contain(x => x is AspNetCoreApplicationPlugin);
		}

		[Fact]
		public void GetRequestScope_WitnNewRequestScope_ResolvesToRequestScope()
		{
			(var applicationPlugin, var context, _) = GetApplicationPluginContext();

			using (var requestScope = new RequestScope())
			{				
				var scope = applicationPlugin.GetRequestScope(context);
				scope.Should().NotBeNull().And.Be(requestScope);
			}
		}

		[Fact]
		public void GetRequestScope_WithoutRequestScope_ThrowsException()
		{
			(var applicationPlugin, var context, _) = GetApplicationPluginContext();
			
			Action action = () => applicationPlugin.GetRequestScope(context);
			action.Should().Throw<ActivationException>();
		}

		private (AspNetCoreApplicationPlugin plugin, IContext context, IKernel kernel) GetApplicationPluginContext()
		{
			var kernel = CreateKernel(new ServiceCollection(), CreateDefaultSettings());
			var applicationPlugin = kernel.Components.GetAll<INinjectHttpApplicationPlugin>().OfType<AspNetCoreApplicationPlugin>().Single();
			var contextMock = new Mock<IContext>();
			contextMock.Setup(ctx => ctx.Parameters).Returns(new List<IParameter>());

			return (applicationPlugin, contextMock.Object, kernel);
		}
	}
}
