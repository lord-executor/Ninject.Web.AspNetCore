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

		[Theory]
		[MemberData(nameof(ResolverData))]
		public void BindingsFromKernel_ResolveWarrior_FailsWithActivationException(Resolver resolver)
		{
			var kernel = CreateDuplicateBindings();

			if (resolver.ResolveIsOptional)
			{
				resolver.Resolve<IWarrior>(kernel).Should().BeNull();
			}
			else
			{
				Action action = () => resolver.Resolve<IWarrior>(kernel);
				action.Should().Throw<ActivationException>().WithMessage("Error activating IWarrior*");
			}
		}

		[Fact]
		public void BindingsFromKernel_ResolveWeaponDependency_FailsWithActivationException()
		{
			var kernel = CreateDuplicateBindings();

			Action action = () => kernel.Get<IWarrior>(metadata => metadata.Get(nameof(Knight), false));
			action.Should().Throw<ActivationException>().WithMessage("Error activating IWeapon*");
		}

		[Theory]
		[MemberData(nameof(ResolverData))]
		public void BindingsMixedWarriorDescriptors_ResolveWarrior_FailsWithActivationException(Resolver resolver)
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), _ => new Ninja("Shadow"), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IWarrior), typeof(Knight), ServiceLifetime.Transient));
			var kernel = CreateKernel(collection);
			kernel.Bind<IWeapon>().To<Lance>();
			kernel.Bind<IWeapon>().To<Longsword>().WithMetadata(nameof(Longsword), true);

			// IWarrior is resolved to Knight because of the "latest" binding, but the two weapon bindings are equivalent
			// and lead to an activation exception
			if (resolver.ResolveIsOptional)
			{
				resolver.Resolve<IWarrior>(kernel).Should().BeNull();
			}
			else
			{
				Action action = () => resolver.Resolve<IWarrior>(kernel);
				action.Should().Throw<ActivationException>().WithMessage("Error activating IWeapon*");
			}
		}

		[Fact]
		public void BindingsMixedWeaponDescriptors_ResolveWarrior_FailsWithActivationException()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWeapon), typeof(Lance), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IWeapon), typeof(Longsword), ServiceLifetime.Transient));
			var kernel = CreateKernel(collection);
			kernel.Bind<IWarrior>().ToConstant(new Ninja("Shadow"));
			kernel.Bind<IWarrior>().To<Knight>().WithMetadata(nameof(Knight), true);

			// IWarrior is cannot be resolved because the two Kernel bindings are equivalent
			Action action = () => kernel.Get<IWarrior>();
			action.Should().Throw<ActivationException>().WithMessage("Error activating IWarrior*");

			// IWeapon binding still works if IWarrior binding is constrained to only match one binding
			var warrior = kernel.Get<IWarrior>(metadata => metadata.Get(nameof(Knight), false));
			warrior.Should().NotBeNull();
			warrior.Should().BeOfType<Knight>();
			(warrior as Knight).Weapon.Should().BeOfType<Longsword>();
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
			var kernel = CreateKernel(new ServiceCollection());
			kernel.Bind<IWarrior>().ToConstant(new Ninja("Shadow"));
			kernel.Bind<IWarrior>().To<Knight>().WithMetadata(nameof(Knight), true);
			kernel.Bind<IWeapon>().To<Lance>();
			kernel.Bind<IWeapon>().To<Longsword>().WithMetadata(nameof(Longsword), true);

			return kernel;
		}

		public enum ResolveType
		{
			ServiceProvider,
			ServiceProviderRequired,
			KernelGet,
			KernelTryGet,
		}

		public class Resolver
		{
			public ResolveType ResolveType { get; }

			public bool ResolveIsOptional => ResolveType == ResolveType.ServiceProvider || ResolveType == ResolveType.KernelTryGet;

			public Resolver(ResolveType resolveType)
			{
				ResolveType = resolveType;
			}
			public T Resolve<T>(IKernel kernel)
			{
				switch (ResolveType)
				{
					case ResolveType.ServiceProvider:
						return kernel.Get<IServiceProvider>().GetService<T>();
					case ResolveType.ServiceProviderRequired:
						return kernel.Get<IServiceProvider>().GetRequiredService<T>();
					case ResolveType.KernelTryGet:
						return kernel.TryGet<T>();

					default:
						return kernel.Get<T>();
				}
			}

			public IEnumerable<T> ResolveAll<T>(IKernel kernel)
			{
				switch (ResolveType)
				{
					case ResolveType.ServiceProvider:
					case ResolveType.ServiceProviderRequired:
						return kernel.Get<IServiceProvider>().GetServices<T>();

					default:
						return kernel.GetAll<T>();
				}
			}
		}

		public static TheoryData<Resolver> ResolverData => new TheoryData<Resolver>
		{
			{ new Resolver(ResolveType.ServiceProvider) },
			{ new Resolver(ResolveType.ServiceProviderRequired) },
			{ new Resolver(ResolveType.KernelTryGet) },
			{ new Resolver(ResolveType.KernelGet) },
		};
	}
}
