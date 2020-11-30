using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ninject.Web.AspNetCore.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit.AspNetCore
{
	public class ServiceScopeTest
	{
		[Fact]
		public void DisposedServiceProviderThrowsException()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), typeof(Samurai), ServiceLifetime.Transient));
			var provider = collection.BuildServiceProvider();

			var service = provider.GetRequiredService<IWarrior>();

			provider.Dispose();

			Action action = () => provider.GetRequiredService<IWarrior>();
			action.Should().Throw<ObjectDisposedException>();
		}

		[Theory]
		[InlineData(typeof(Ninja), true, 2)]
		[InlineData(typeof(Samurai), false, 1)]
		public void ServiceProviderAlwaysHasRootScope(Type serviceType, bool isScoped, int instanceCount)
		{
			var count = 0;
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(Ninja), _ => { count++; return new Ninja("The Unknown"); }, ServiceLifetime.Scoped));
			collection.Add(new ServiceDescriptor(typeof(Samurai), _ => { count++; return new Samurai(); }, ServiceLifetime.Singleton));
			var provider = collection.BuildServiceProvider();
			var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

			var service = provider.GetRequiredService(serviceType);
			count.Should().Be(1);

			using (var scope = scopeFactory.CreateScope())
			{
				var inner = scope.ServiceProvider.GetRequiredService(serviceType);
				count.Should().Be(instanceCount);
				(inner == service).Should().Be(!isScoped);
			}

			var outer = provider.GetRequiredService(serviceType);
			outer.Should().BeSameAs(service);
			count.Should().Be(instanceCount);
		}

		[Theory]
		[InlineData(typeof(Ninja))]
		[InlineData(typeof(Samurai))]
		public void ScopedServicesWithDifferentScopesCreateOneInstancePerScope(Type serviceType)
		{
			var count = 0;
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(Ninja), _ => { count++; return new Ninja("The Unknown"); }, ServiceLifetime.Scoped));
			collection.Add(new ServiceDescriptor(typeof(Samurai), _ => { count++; return new Samurai(); }, ServiceLifetime.Transient));
			var provider = collection.BuildServiceProvider();
			var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

			IWarrior first, second;

			using (var scope = scopeFactory.CreateScope())
			{
				first = (IWarrior)scope.ServiceProvider.GetRequiredService(serviceType);
				count.Should().Be(1);
			}

			using (var scope = scopeFactory.CreateScope())
			{
				second = (IWarrior)scope.ServiceProvider.GetRequiredService(serviceType);
				count.Should().Be(2);
				second.Should().NotBeSameAs(first);
			}
		}

		[Fact]
		public void InstancesAreResolvedAsThemselves()
		{
			var collection = new ServiceCollection();
			var theSamurai = new Samurai();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), theSamurai));

			var provider = collection.BuildServiceProvider();

			provider.GetService<IWarrior>().Should().BeSameAs(theSamurai);
		}

		[Fact]
		public void ScopedResolutionRespectsScopeLifetime()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(Canary), typeof(Canary), ServiceLifetime.Scoped));

			var provider = collection.BuildServiceProvider();
			var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

			var first = provider.GetService<Canary>();
			Canary second;

			using (var scope = scopeFactory.CreateScope())
			{
				second = scope.ServiceProvider.GetService<Canary>();
				first.Should().NotBeSameAs(second);
				second.IsDisposed.Should().BeFalse();
			}

			second.IsDisposed.Should().BeTrue();

			var fourth = provider.GetService<Canary>();
			first.Should().BeSameAs(fourth);
		}
	}
}
