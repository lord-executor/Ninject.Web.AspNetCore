using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Web.AspNetCore.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class ServiceProviderTest
	{
				
		[Fact]
		public void ServiceProvider_CreatedSuccessfully()
		{
			StandardKernel kernel = CreateTestKernel();
			NinjectServiceProviderBuilder builder = CreateServiceProviderBuilder(kernel);

			var provider = builder.Build();
			
			provider.Should().NotBeNull().And.BeOfType(typeof(NinjectServiceProvider));
		}		

		[Fact]
		public void OptionalExisting_SingleServiceResolved()
		{
			StandardKernel kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>();
			var provider = CreateServiceProvider(kernel);
			
			provider.GetService(typeof(IWarrior)).Should().NotBeNull().And.BeOfType(typeof(Samurai));
		}

		[Fact]
		public void OptionalNonExisting_SingleServiceResolvedToNull()
		{
			StandardKernel kernel = CreateTestKernel();
			var provider = CreateServiceProvider(kernel);
			
			provider.GetService(typeof(IWarrior)).Should().BeNull();
		}

		[Fact]
		public void OptionalExistingMultipleServices_ResolvedQueriedAsList()
		{
			StandardKernel kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>();
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test"));
			var provider = CreateServiceProvider(kernel);
			
			var result = provider.GetService(typeof(IList<IWarrior>)) as IEnumerable<IWarrior>;
			
			result.Should().NotBeNull();
			var resultList = result.ToList();
			resultList.Should().HaveCount(2);
			resultList.Should().Contain(x => x is Samurai);
			resultList.Should().Contain(x => x is Ninja);
		}

		[Fact]
		public void OptionalExistingMultipleServices_ResolvesToNull()
		{
			StandardKernel kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>();
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test"));
			var provider = CreateServiceProvider(kernel);

			provider.GetService(typeof(IWarrior)).Should().BeNull();
		}

		[Fact]
		public void RequiredExistingSingleServiceResolved()
		{
			StandardKernel kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>();
			var provider = CreateServiceProvider(kernel);

			provider.GetRequiredService(typeof(IWarrior)).Should().NotBeNull().And.BeOfType(typeof(Samurai));
		}

		[Fact]
		public void RequiredNonExistingSingleServiceResolvedToException()
		{
			StandardKernel kernel = CreateTestKernel();
			var provider = CreateServiceProvider(kernel);

			Action action = () => provider.GetRequiredService(typeof(IWarrior));
			action.Should().Throw<ActivationException>().WithMessage("*No matching bindings are available*");
		}

		[Fact]
		public void RequiredExistingMultipleServicesResolvedQueriedAsList()
		{
			StandardKernel kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>();
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test"));
			var provider = CreateServiceProvider(kernel);

			var result = provider.GetRequiredService(typeof(IList<IWarrior>)) as IEnumerable<IWarrior>;

			result.Should().NotBeNull();
			var resultList = result.ToList();
			resultList.Should().HaveCount(2);
			resultList.Should().Contain(x => x is Samurai);
			resultList.Should().Contain(x => x is Ninja);
		}

		[Fact]
		public void RequiredExistingMultipleServicesResolvedToExceptionWhenNotQueriedAsList()
		{
			StandardKernel kernel = CreateTestKernel();
			kernel.Bind<IWarrior>().To<Samurai>();
			kernel.Bind<IWarrior>().ToConstant(new Ninja("test"));
			var provider = CreateServiceProvider(kernel);

			Action action = () => provider.GetRequiredService(typeof(IWarrior));
			action.Should().Throw<ActivationException>().WithMessage("*More than one matching bindings are available*");
		}

		private IServiceProvider CreateServiceProvider(IKernel kernel)
		{
			NinjectServiceProviderBuilder builder = CreateServiceProviderBuilder(kernel);
			var provider = builder.Build();
			return provider;
		}

		private NinjectServiceProviderBuilder CreateServiceProviderBuilder(IKernel kernel)
		{
			var collection = new ServiceCollection();
			var factory = new NinjectServiceProviderFactory(kernel);
			var builder = factory.CreateBuilder(collection);
			return builder;
		}

		private StandardKernel CreateTestKernel()
		{
			var kernel = new StandardKernel(new NinjectSettings() { LoadExtensions = false });
			kernel.Load(typeof(AspNetCoreApplicationPlugin).Assembly);
			return kernel;
		}




	}
}
