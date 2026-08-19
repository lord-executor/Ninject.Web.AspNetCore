using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ninject.Web.AspNetCore.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{

#if NET8_0_OR_GREATER

	public class ServiceProviderKeyedTest
	{

		[Fact]
		public void OptionalExising_ServiceKeyNullResolvedAsUnkeyed()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), null, typeof(Samurai), ServiceLifetime.Transient));
			var kernel = CreateTestKernel(collection);
			var provider = CreateServiceProvider(kernel);

			var warrior = provider.GetKeyedService(typeof(Samurai), null);
			warrior.Should().NotBeNull().And.BeOfType(typeof(Samurai));
		}

		[Fact]
		public void OptionalExising_SingleServiceInjectedServiceKeyResolved()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), "Ninja", typeof(KeyedNinja), ServiceLifetime.Transient));
			var kernel = CreateTestKernel(collection);
			var provider = CreateServiceProvider(kernel);

			var warrior = provider.GetKeyedService(typeof(IWarrior), "Ninja");
			warrior.Should().NotBeNull().And.BeOfType(typeof(KeyedNinja)).And.Match(x => ((KeyedNinja)x).Key.ToString() == "Ninja");
		}

		[Fact]
		public void OptionalExisingWithKeyedChildren_SingleServiceResolved()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), "Ninja", typeof(NinjaWithKeyedWeapon), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IKeyedWeaponStorage), "Storage", typeof(KeyedWeaponStorage), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IWeapon), "Longsword", typeof(Longsword), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IWeapon), "Lance", typeof(Lance), ServiceLifetime.Transient));
			var kernel = CreateTestKernel(collection);
			var provider = CreateServiceProvider(kernel);

			var warrior = provider.GetKeyedService(typeof(IWarrior), "Ninja");
			warrior.Should().NotBeNull().And.BeOfType(typeof(NinjaWithKeyedWeapon)).And.Match(x => ((NinjaWithKeyedWeapon)x).Weapon.Type == nameof(Longsword));
			((NinjaWithKeyedWeapon)warrior).Storage.Should().NotBeNull().And.BeOfType(typeof(KeyedWeaponStorage)).And
				.Match(x => ((KeyedWeaponStorage)x).Weapon.Type == nameof(Lance));
		}

		[Fact]
		public void OptionalKeyedServiceCollectionExisting_CorrectServiceResolved()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), "Samurai", typeof(Samurai), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IWarrior), "Ninja1", new Ninja("test")));
			collection.Add(new ServiceDescriptor(typeof(IWarrior), "Ninja2",
				(provider, key) => new Ninja("test:" + key.ToString()), ServiceLifetime.Transient));
			var kernel = CreateTestKernel(collection);
			var provider = CreateServiceProvider(kernel);

			provider.GetKeyedService(typeof(IWarrior), "Samurai").Should().NotBeNull().And.BeOfType(typeof(Samurai));
			provider.GetKeyedService(typeof(IWarrior), "Ninja1").Should().NotBeNull().And.BeOfType(typeof(Ninja)).
				And.Match(x => ((Ninja)x).Name == "test");
			var ninja2First = provider.GetKeyedService(typeof(IWarrior), "Ninja2");
			var ninja2Second = provider.GetKeyedService(typeof(IWarrior), "Ninja2");
			ninja2First.Should().NotBeNull().And.BeOfType(typeof(Ninja)).
				And.Match(x => ((Ninja)x).Name == "test:Ninja2");
			ninja2Second.Should().NotBeNull().And.BeOfType(typeof(Ninja)).
				And.Match(x => ((Ninja)x).Name == "test:Ninja2");
			ninja2First.Should().NotBeSameAs(ninja2Second);
		}

		[Fact]
		public void OptionalKeyedNinjectDirectBindingExisting_CorrectServiceResolved()
		{
			var kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Samurai"));
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Ninja"));
			var provider = CreateServiceProvider(kernel);

			provider.GetKeyedService(typeof(IWarrior), "Samurai").Should().NotBeNull().And.BeOfType(typeof(Samurai));
			provider.GetKeyedService(typeof(IWarrior), "Ninja").Should().NotBeNull().And.BeOfType(typeof(Ninja));
		}

		[Fact]
		public void OptionalKeyedNonExisting_SingleServiceResolvedToNull()
		{
			var kernel = CreateTestKernel();
			var provider = CreateServiceProvider(kernel);

			provider.GetKeyedService(typeof(IWarrior), "Samurai").Should().BeNull();
		}

		[Fact]
		public void OptionalExistingMultipleKeydServices_ResolvedQueriedAsList()
		{
			var kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Warrior"));
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Warrior"));
			var provider = CreateServiceProvider(kernel);

			var result = provider.GetKeyedService(typeof(IList<IWarrior>), "Warrior") as IEnumerable<IWarrior>;

			result.Should().NotBeNull();
			var resultList = result.ToList();
			resultList.Should().HaveCount(2);
			resultList.Should().Contain(x => x is Samurai);
			resultList.Should().Contain(x => x is Ninja);
		}

		[Fact]
		public void OptionalExistingMultipleKeydServices_NotResolvedAsListNonKeyed()
		{
			var kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Samurai"));
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Ninja"));
			var provider = CreateServiceProvider(kernel);

			var result = provider.GetService(typeof(IList<IWarrior>)) as IEnumerable<IWarrior>;

			result.Should().NotBeNull();
			var resultList = result.ToList();
			resultList.Should().HaveCount(0);
		}

		[Fact]
		public void ExistingMultipleServices_ResolvesNonKeyedToNull()
		{
			var kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Samurai"));
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Ninja"));
			var provider = CreateServiceProvider(kernel);

			provider.GetService(typeof(IWarrior)).Should().BeNull();
		}

		[Fact]
		public void RequiredKeyedServiceCollectionExisting_CorrectServiceResolved()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), "Samurai", typeof(Samurai), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IWarrior), "Ninja", new Ninja("test")));
			var kernel = CreateTestKernel(collection);
			var provider = CreateServiceProvider(kernel);

			provider.GetRequiredKeyedService(typeof(IWarrior), "Ninja").Should().NotBeNull().And.BeOfType(typeof(Ninja));
			provider.GetRequiredKeyedService(typeof(IWarrior), "Samurai").Should().NotBeNull().And.BeOfType(typeof(Samurai));

		}

		[Fact]
		public void RequiredKeyedNinjectDirectBindingExisting_CorrectServiceResolved()
		{
			var kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Samurai"));
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Ninja"));
			var provider = CreateServiceProvider(kernel);

			provider.GetRequiredKeyedService(typeof(IWarrior), "Samurai").Should().NotBeNull().And.BeOfType(typeof(Samurai));
			provider.GetRequiredKeyedService(typeof(IWarrior), "Ninja").Should().NotBeNull().And.BeOfType(typeof(Ninja));
		}

		[Fact]
		public void RequiredKeyedNonExisting_SingleServiceResolvedToException()
		{
			var kernel = CreateTestKernel();
			var provider = CreateServiceProvider(kernel);

			Action action = () => provider.GetRequiredKeyedService(typeof(IWarrior), "Samurai");
			action.Should().Throw<InvalidOperationException>().WithInnerException<ActivationException>().WithMessage("*No matching bindings are available*");
		}

		[Fact]
		public void RequiredExistingMultipleKeydServices_ResolvedQueriedAsList()
		{
			var kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Warrior"));
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Warrior"));
			var provider = CreateServiceProvider(kernel);

			var result = provider.GetRequiredKeyedService(typeof(IList<IWarrior>), "Warrior") as IEnumerable<IWarrior>;

			result.Should().NotBeNull();
			var resultList = result.ToList();
			resultList.Should().HaveCount(2);
			resultList.Should().Contain(x => x is Samurai);
			resultList.Should().Contain(x => x is Ninja);
		}

		[Fact]
		public void RequiredExistingMultipleKeydServices_NotResolvedAsListNonKeyed()
		{
			var kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Samurai"));
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Ninja"));
			var provider = CreateServiceProvider(kernel);

			var result = provider.GetRequiredService(typeof(IList<IWarrior>)) as IEnumerable<IWarrior>;

			result.Should().NotBeNull();
			var resultList = result.ToList();
			resultList.Should().HaveCount(0);
		}

		[Fact]
		public void ExistingMultipleServices_ResolvesNonKeyedToException()
		{
			var kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Samurai"));
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Ninja"));
			var provider = CreateServiceProvider(kernel);

			Action action = () => provider.GetRequiredService(typeof(IWarrior));
			action.Should().Throw<InvalidOperationException>().WithInnerException<ActivationException>().WithMessage("*No matching bindings are available, and the type is not self-bindable.*");
		}

		private IServiceProvider CreateServiceProvider(AspNetCoreKernel kernel)
		{
			NinjectServiceProviderBuilder builder = CreateServiceProviderBuilder(kernel);
			var provider = builder.Build();
			return provider;
		}

		private NinjectServiceProviderBuilder CreateServiceProviderBuilder(AspNetCoreKernel kernel)
		{
			var collection = new ServiceCollection();
			var factory = new NinjectServiceProviderFactory(kernel);
			var builder = factory.CreateBuilder(collection);
			return builder;
		}

		private AspNetCoreKernel CreateTestKernel(IServiceCollection collection = null)
		{
			var kernel = new AspNetCoreKernel(new NinjectSettings() { LoadExtensions = false });
			kernel.Load(typeof(AspNetCoreApplicationPlugin).Assembly);
			if (collection != null)
			{
				new ServiceCollectionAdapter().Populate(kernel, collection);
			}

			return kernel;
		}

	}
#endif
}
