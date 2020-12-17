using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ninject.Web.AspNetCore.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class DuplicateDescriptorTest : TestKernelContext
	{
		[Theory]
		[MemberData(nameof(ResolverData))]
		public void BindingsFromDescriptors_ResolveWarrior_ResolvesLatestWithLatestDependency(Resolver resolver)
		{
			var kernel = CreateDuplicateDescriptors();

			var warrior = resolver.Resolve<IWarrior>(kernel);
			warrior.Should().NotBeNull();
			warrior.Should().BeOfType<Knight>();
			(warrior as Knight).Weapon.Should().BeOfType<Longsword>();
		}

		[Theory]
		[MemberData(nameof(ResolverData))]
		public void BindingsFromDescriptors_ResolveAllWarriors_ResolvesAllWithLatestDependency(Resolver resolver)
		{
			var kernel = CreateDuplicateDescriptors();

			var warriors = resolver.ResolveAll<IWarrior>(kernel);
			warriors.Should().HaveCount(2);
			warriors.OfType<Knight>().Should().HaveCount(1);
			warriors.OfType<Knight>().Single().Weapon.Should().BeOfType<Longsword>();
		}

		[Theory]
		[MemberData(nameof(ResolverData))]
		public void BindingsFromDescriptors_ResolveAllWarriors_OrderOfDescriptorsIsPreserved(Resolver resolver)
		{
			var kernel = CreateDuplicateDescriptors();

			var warriors = resolver.ResolveAll<IWarrior>(kernel);
			warriors.Should().HaveCount(2);
			// Yes. ASP.NET Core actually works under the assumptions that multi-injection happens in
			// the same order as the registration of the descriptors and in situations like when resolving
			// and running IEnumerable<IConfigureOptions<KestrelServerOptions>>, it will fail when that
			// order is not preserved.
			warriors.Select(w => w.GetType()).Should().ContainInOrder(
				typeof(Ninja),
				typeof(Knight)
			);
		}

		[Fact]
		public void BindingsFromKernel_ResolveWarrior_FailsWithActivationException()
		{
			var kernel = CreateDuplicateBindings();

			Action action = () => kernel.Get<IWarrior>();
			action.Should().Throw<ActivationException>().WithMessage("Error activating IWarrior*");
		}

		[Fact]
		public void BindingsFromKernel_ResolveWeaponDependency_FailsWithActivationException()
		{
			var kernel = CreateDuplicateBindings();

			Action action = () => kernel.Get<IWarrior>(metadata => metadata.Get(nameof(Knight), false));
			action.Should().Throw<ActivationException>().WithMessage("Error activating IWeapon*");
		}

		private IKernel CreateDuplicateDescriptors()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), _ => new Ninja("Shadow"), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IWarrior), typeof(Knight), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IWeapon), typeof(Lance), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IWeapon), typeof(Longsword), ServiceLifetime.Transient));

			return CreateKernel(collection);
		}

		private IKernel CreateDuplicateBindings()
		{
			var kernel = new AspNetCoreKernel(new NinjectSettings() { LoadExtensions = false });
			kernel.Bind<IWarrior>().ToConstant(new Ninja("Shadow"));
			kernel.Bind<IWarrior>().To<Knight>().WithMetadata(nameof(Knight), true);
			kernel.Bind<IWeapon>().To<Lance>();
			kernel.Bind<IWeapon>().To<Longsword>().WithMetadata(nameof(Longsword), true);

			return kernel;
		}

		public enum ResolveType
		{
			ServiceProvider,
			Kernel,
		}

		public class Resolver
		{
			private readonly ResolveType _resolveType;

			public Resolver(ResolveType resolveType)
			{
				_resolveType = resolveType;
			}
			public T Resolve<T>(IKernel kernel)
			{
				switch (_resolveType)
				{
					case ResolveType.ServiceProvider:
						return kernel.Get<IServiceProvider>().GetService<T>();

					default:
						return kernel.Get<T>();
				}
			}

			public IEnumerable<T> ResolveAll<T>(IKernel kernel)
			{
				switch (_resolveType)
				{
					case ResolveType.ServiceProvider:
						return kernel.Get<IServiceProvider>().GetServices<T>();

					default:
						return kernel.GetAll<T>();
				}
			}
		}

		public static TheoryData<Resolver> ResolverData => new TheoryData<Resolver>
		{
			{ new Resolver(ResolveType.ServiceProvider) },
			{ new Resolver(ResolveType.Kernel) },
		};
	}
}
