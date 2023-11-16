using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ninject.Web.AspNetCore.Test.Fakes;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit;

public class ServiceProviderScopeTest : TestKernelContext
{
	[Theory]
	[MemberData(nameof(ServiceConfigurations))]
	public void ScopedServices_ResolvedInScope_ShouldBeDifferent(ServiceCollection serviceCollection)
	{
		var kernel = CreateKernel(serviceCollection);
		var provider = kernel.Get<IServiceProvider>();

		var rootKnight = provider.GetRequiredService<Knight>();

		using (var scope = provider.CreateScope())
		{
			// The IWeapon instantiated inside of this scope should also be tied to this scope when created through
			// a ServiceDescriptor.ImplementationFactory
			var levelOneKnight = scope.ServiceProvider.GetRequiredService<Knight>();
			AssertNotSameKnightAndWeapon(levelOneKnight, rootKnight);
			
			var levelOneKnight2 = scope.ServiceProvider.GetRequiredService<Knight>();
			AssertSameKnightAndWeapon(levelOneKnight2, levelOneKnight);
		}

		var rootKnight2 = provider.GetRequiredService<Knight>();
		AssertSameKnightAndWeapon(rootKnight2, rootKnight);
	}

	private void AssertSameKnightAndWeapon(Knight value, Knight expected)
	{
		value.Should().BeSameAs(expected);
		value.Weapon.Should().BeSameAs(expected.Weapon);
	}
	
	private void AssertNotSameKnightAndWeapon(Knight value, Knight expected)
	{
		value.Should().NotBeSameAs(expected);
		value.Weapon.Should().NotBeSameAs(expected.Weapon);
	}
	
	public static TheoryData<ServiceCollection> ServiceConfigurations => new TheoryData<ServiceCollection>
	{
		{
			new ServiceCollection() {
				new ServiceDescriptor(typeof(Knight), typeof(Knight), ServiceLifetime.Scoped),
				new ServiceDescriptor(typeof(IWeapon), typeof(Longsword), ServiceLifetime.Scoped)
			}
		},
		{
			new ServiceCollection() {
				new ServiceDescriptor(typeof(Knight), typeof(Knight), ServiceLifetime.Scoped),
				new ServiceDescriptor(typeof(Longsword), typeof(Longsword), ServiceLifetime.Scoped),
				// See https://github.com/lord-executor/Ninject.Web.AspNetCore/issues/12
				// The difference here is the use of a descriptor with an implementation factory where the implementation
				// must make sure that the correct _scoped_ IServiceProvider instance is passed to the factory
				new ServiceDescriptor(typeof(IWeapon),serviceProvider => serviceProvider.GetService<Longsword>(), ServiceLifetime.Scoped)
			}
		},
	};
}