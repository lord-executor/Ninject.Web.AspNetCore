using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Web.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
		public void GetRequestScope_WithoutRequestScopeAndThrowConfiguration_ThrowsExceptionByDefault()
		{
			var settings = CreateDefaultSettings();
			settings.SetMissingRequestScopeBehavior(MissingRequestScopeBehaviorType.Throw);
			(var applicationPlugin, var context, _) = GetApplicationPluginContext();

			Action action = () => applicationPlugin.GetRequestScope(context);
			action.Should().Throw<ActivationException>();
		}

		[Fact]
		public void GetRequestScope_WithoutRequestScopeAndUseKernelConfiguration_UsesKernelAsScope()
		{
			var settings = CreateDefaultSettings();
			settings.SetMissingRequestScopeBehavior(MissingRequestScopeBehaviorType.UseKernel);
			(var applicationPlugin, var context, var kernel) = GetApplicationPluginContext(settings);

			var scope = applicationPlugin.GetRequestScope(context);
			scope.Should().NotBeNull().And.Be(kernel);
		}

		[Fact]
		public void GetRequestScope_WithoutRequestScopeAndUseTransientConfiguration_UsesNullAsScope()
		{
			var settings = CreateDefaultSettings();
			settings.SetMissingRequestScopeBehavior(MissingRequestScopeBehaviorType.UseTransient);
			(var applicationPlugin, var context, _) = GetApplicationPluginContext(settings);

			var scope = applicationPlugin.GetRequestScope(context);
			scope.Should().BeNull();
		}

		private (AspNetCoreApplicationPlugin plugin, IContext context, IKernel kernel) GetApplicationPluginContext(NinjectSettings settings = null)
		{
			var kernel = CreateKernel(new ServiceCollection(), settings ?? CreateDefaultSettings());
			var applicationPlugin = kernel.Components.GetAll<INinjectHttpApplicationPlugin>().OfType<AspNetCoreApplicationPlugin>().Single();
			var contextMock = new Mock<IContext>();
			contextMock.Setup(ctx => ctx.Parameters).Returns(new List<IParameter>());

			return (applicationPlugin, contextMock.Object, kernel);
		}
	}
}
