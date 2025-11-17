using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ninject.Web.AspNetCore.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.ServiceProviderReference
{
	public class ServiceProviderReferenceTest
	{
		[Fact]
		public void ServiceProvider_ProvideServiceScopeFactory_HasNoEffect()
		{
			var scopeFactoryMock = new Mock<IServiceScopeFactory>();

			var provider = CreateServiceProvider(collection => {
				collection.AddSingleton<IServiceScopeFactory>(scopeFactoryMock.Object);
			});

			// Due to the implementation of the ServiceProviderEngine base class which always adds _itself_ as the
			// service scope factory as the last step after configuring the services from descriptors, the mock provided
			// above will never be used. See
			// * ServiceProviderEngine.ServiceProviderEngine constructor
			// * ServiceProviderServiceExtensions.CreateScope
			var factory = provider.GetRequiredService<IServiceScopeFactory>();
			
			factory.Should().NotBeNull().And.NotBe(scopeFactoryMock.Object);
			factory.GetType().Name.Should().Contain("ServiceProviderEngine");
		}

		[Fact]
		public void ServiceProvider_WithScopedService_CreatesImplicitRootScope()
		{
			var provider = CreateServiceProvider();

			// See the comment on MissingRequestScopeBehaviorType
			var first = provider.GetService<Ninja>();
			var second = provider.GetService<Ninja>();

			first.Should().Be(second);
		}

		[Fact]
		public void ServiceProvider_TransientService_CreatesNewInstanceEveryTime()
		{
			var provider = CreateServiceProvider();
			var instances = CreateInstances<Samurai>(provider);

			for (int i = 0; i < instances.Count; i++)
			{
				var candidate = instances[i];
				instances.Skip(i + 1).Should().NotContain(candidate);
			}
		}

		[Fact]
		public void ServiceProvider_ScopedService_CreatesInstancePerScope()
		{
			var provider = CreateServiceProvider();
			var instances = CreateInstances<Ninja>(provider);

			instances.Select(n => n.Name).Should().BeEquivalentTo(
				NinjaNames[0],
				NinjaNames[0],
				NinjaNames[1],
				NinjaNames[1],
				NinjaNames[0],
				NinjaNames[0]
			);
		}

		[Fact]
		public void ServiceProvider_SingletonService_CreatesSingleInstance()
		{
			var provider = CreateServiceProvider();
			var instances = CreateInstances<Knight>(provider);

			instances.Aggregate((a, b) => a == b ? b : null).Should().NotBeNull();
		}

		[Fact]
		public void ServiceProvider_ScopedServiceResolveServiceProvider_ReturnsScopedServiceProvider()
		{
			// Interesting side-note here: The ServiceProvider that is created is not actually the same object as when
			// the IServiceProvider is requested from that provider.
			var provider = CreateServiceProvider();

			var rootProvider = provider.GetService<IServiceProvider>();
			// Yes, requesting IServiceProvider will actually return a ServiceProviderEngineScope which is the root
			// scope of this provider.
			rootProvider.Should().BeAssignableTo<IServiceScope>();
			var rootScope = rootProvider as IServiceScope;

			using (var scope = provider.CreateScope())
			{
				scope.Should().NotBe(rootScope);
				scope.ServiceProvider.Should().NotBe(provider);
				scope.ServiceProvider.Should().NotBe(rootProvider);
				// Requesting an IServiceProvider from a scoped service provider will return that same scoped
				// service provider.
				scope.ServiceProvider.GetService<IServiceProvider>().Should().Be(scope.ServiceProvider);
			}
		}

		private IServiceProvider CreateServiceProvider(Action<ServiceCollection> serviceConfig = null)
		{
			var collection = new ServiceCollection();

			collection.AddTransient<Samurai>();

			var ninjaIndex = 0;
			collection.AddScoped(_ => new Ninja(NinjaNames[ninjaIndex++]));

			collection.AddSingleton(_ => new Knight(new Longsword()));

			serviceConfig?.Invoke(collection);
			return collection.BuildServiceProvider();
		}

		private IList<T> CreateInstances<T>(IServiceProvider provider)
		{
			var instances = new List<T>();

			instances.Add(provider.GetService<T>());
			instances.Add(provider.GetService<T>());

			using (var scope = provider.CreateScope())
			{
				instances.Add(scope.ServiceProvider.GetService<T>());
				instances.Add(scope.ServiceProvider.GetService<T>());
			}

			instances.Add(provider.GetService<T>());
			instances.Add(provider.GetService<T>());

			return instances;
		}

		private static IList<string> NinjaNames = new List<string>
		{
			"Shadow",
			"Scorpion",
			"Vega",
		};
	}
}
