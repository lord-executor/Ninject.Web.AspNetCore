using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ninject.Web.AspNetCore.Test.Fakes;
using Ninject.Web.Common;
using System;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class PopulateFromServiceCollectionTest : TestKernelContext
	{
		
		[Fact]
		public void InstancesAreConvertedToConstants()
		{
			var collection = new ServiceCollection();
			var theSamurai = new Samurai();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), theSamurai));

			var kernel = CreateKernel(collection);

			kernel.Get<IWarrior>().Should().BeSameAs(theSamurai);
		}

		[Fact]
		public void SingletonRegistrationIsConvertedToSingleton()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), typeof(Samurai), ServiceLifetime.Singleton));

			var kernel = CreateKernel(collection);

			var first = kernel.Get<IWarrior>();
			var second = kernel.Get<IWarrior>();
			first.Should().BeSameAs(second).And.BeOfType(typeof(Samurai));
		}

		[Fact]
		public void TransientRegistrationIsConvertedToTransient()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), typeof(Samurai), ServiceLifetime.Transient));

			var kernel = CreateKernel(collection);

			var first = kernel.Get<IWarrior>();
			var second = kernel.Get<IWarrior>();
			first.Should().NotBeSameAs(second).And.BeOfType(typeof(Samurai));
		}		

		[Fact]
		public void ScopedRegistrationIsConvertedToInRequestScope()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), typeof(Samurai), ServiceLifetime.Scoped));

			var kernel = CreateKernel(collection);			

			IWarrior first;
			using (var scope1 = new NinjectServiceScope(kernel))
			{
				first = kernel.Get<IWarrior>();
				var second = kernel.Get<IWarrior>();
				first.Should().BeSameAs(second).And.BeOfType(typeof(Samurai));
			}
			using (var scope2 = new NinjectServiceScope(kernel))
			{
				var third = kernel.Get<IWarrior>();
				third.Should().NotBeSameAs(first).And.BeOfType(typeof(Samurai));
			}
		}

		[Fact]
		public void FactoryRegistrationConvertedToMethodRegistration()
		{
			var name = "factory created";
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), (x) => { return new Ninja(name); }, ServiceLifetime.Transient));

			var kernel = CreateKernel(collection);

			var first = kernel.Get<IWarrior>();
			first.Name.Should().Be(name);
			first.Should().BeOfType(typeof(Ninja));
		}
	}
}
