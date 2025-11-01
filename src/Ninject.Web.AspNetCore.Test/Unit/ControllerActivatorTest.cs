using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Ninject.Web.AspNetCore.Hosting;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class CustomControllerActivator : IControllerActivator
	{
		public object Create(ControllerContext context)
		{
			throw new System.NotImplementedException();
		}

		public void Release(ControllerContext context, object controller)
		{
			throw new System.NotImplementedException();
		}
	}

	public class ControllerActivatorTest : TestKernelContext
	{

		[Fact]
		public void DefaultControllerActivatorIsServiceProviderBased()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IControllerActivator), new Mock<IControllerActivator>().Object));
			var kernel = CreateKernel(collection);

			kernel.Get<IControllerActivator>().Should().NotBeNull().And.BeOfType(typeof(ServiceBasedControllerActivator));
		}

		[Fact]
		public void CanReplaceDefaultControllerActivator()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IControllerActivator), new Mock<IControllerActivator>().Object));
			var config = new AspNetCoreHostConfiguration();
			config.UseCustomControllerActivator(typeof(CustomControllerActivator));
			var kernel = CreateKernel(collection, config);

			kernel.Get<IControllerActivator>().Should().NotBeNull().And.BeOfType(typeof(CustomControllerActivator));
		}

	}
}
