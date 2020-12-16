using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ninject.Web.AspNetCore.Test.Fakes;
using System;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class DuplicateDescriptorTest : TestKernelContext
	{
		[Fact]
		public void ServiceProviderWithDuplicateDescriptorsResolvesLatest()
		{
			var serviceProvider = CreateDuplicateDescriptors().Get<IServiceProvider>();

			var warrior = serviceProvider.GetService<IWarrior>();
			warrior.Should().NotBeNull();
			warrior.Should().BeOfType<Ninja>();
		}

		[Fact]
		public void KernelGetWithDuplicateDescriptorsThrows()
		{
			var kernel = CreateDuplicateDescriptors();

			Action action = () => kernel.Get<IWarrior>();
			action.Should().Throw<ActivationException>();
		}

		[Fact]
		public void ServiceProviderMultiInjectionWithDuplicateDescriptorsResolvesAll()
		{
			var serviceProvider = CreateDuplicateDescriptors().Get<IServiceProvider>();

			var warriors = serviceProvider.GetServices<IWarrior>();
			warriors.Should().HaveCount(2);
		}

		[Fact]
		public void KernelGetAllWithDuplicateDescriptorsResolvesAll()
		{
			var kernel = CreateDuplicateDescriptors();

			var warriors = kernel.GetAll<IWarrior>();
			warriors.Should().HaveCount(2);
		}

		private IKernel CreateDuplicateDescriptors()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), typeof(Samurai), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IWarrior), _ => new Ninja("Shadow"), ServiceLifetime.Transient));

			return CreateKernel(collection);
		}
	}
}
