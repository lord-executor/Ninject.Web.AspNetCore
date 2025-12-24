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
		public void OptionalKeyedServiceCollectionExisting_CorrectServiceResolved()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior),"Samurai", typeof(Samurai), ServiceLifetime.Transient));
			collection.Add(new ServiceDescriptor(typeof(IWarrior), "Ninja", new Ninja("test")));
			var kernel = CreateTestKernel(collection);
			var provider = CreateServiceProvider(kernel);

			provider.GetKeyedService(typeof(IWarrior), "Ninja").Should().NotBeNull().And.BeOfType(typeof(Ninja));
			provider.GetKeyedService(typeof(IWarrior), "Samurai").Should().NotBeNull().And.BeOfType(typeof(Samurai));

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
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Samurai"));;
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Ninja"));
			var provider = CreateServiceProvider(kernel);

			var result = provider.GetService(typeof(IList<IWarrior>)) as IEnumerable<IWarrior>;

			result.Should().NotBeNull();
			var resultList = result.ToList();
			resultList.Should().HaveCount(2);
			resultList.Should().Contain(x => x is Samurai);
			resultList.Should().Contain(x => x is Ninja);
		}

		[Fact]
		public void ExistingMultipleServices_ResolvesNonKeyedToNull()
		{
			var kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Samurai"));;
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Ninja"));
			var provider = CreateServiceProvider(kernel);

			provider.GetService(typeof(IWarrior)).Should().BeNull();
		}

		[Fact]
		public void RequiredKeyedServiceCollectionExisting_CorrectServiceResolved()
		{
			var collection = new ServiceCollection();
			collection.Add(new ServiceDescriptor(typeof(IWarrior),"Samurai", typeof(Samurai), ServiceLifetime.Transient));
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
			action.Should().Throw<ActivationException>().WithMessage("*No matching bindings are available*");
		}

		[Fact]
		public void RequiredExistingMultipleKeydServices_ResolvedQueriedAsList()
		{
			var kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Samurai"));;
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Ninja"));
			var provider = CreateServiceProvider(kernel);

			var result = provider.GetRequiredService(typeof(IList<IWarrior>)) as IEnumerable<IWarrior>;

			result.Should().NotBeNull();
			var resultList = result.ToList();
			resultList.Should().HaveCount(2);
			resultList.Should().Contain(x => x is Samurai);
			resultList.Should().Contain(x => x is Ninja);
		}

		[Fact]
		public void ExistingMultipleServices_ResolvesNonKeyedToException()
		{
			var kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>().WithMetadata(nameof(ServiceKey), new ServiceKey("Samurai"));;
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test")).WithMetadata(nameof(ServiceKey), new ServiceKey("Ninja"));
			var provider = CreateServiceProvider(kernel);

			Action action = () => provider.GetRequiredService(typeof(IWarrior));
			action.Should().Throw<ActivationException>().WithMessage("*More than one matching bindings are available*");
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
