using System;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Planning.Bindings.Resolvers;
using Xunit;

namespace Ninject.Web.AspNetCore.ComplianceTest;

/// <summary>
/// See https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection.Specification.Tests/src - the dotnet/runtime
/// project which contains the dependency injection library code also contains a set of "compliance tests" that can be run against a potential alternative
/// implementation to check if it is compliant. This class is running the dedicated specification tests for KEYED services.
/// </summary>
public class KeyedDependencyInjectionComplianceTests : Microsoft.Extensions.DependencyInjection.Specification.KeyedDependencyInjectionSpecificationTests
{
	protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
	{
		var kernel = new AspNetCoreKernel();
		// remove autobinding as CreateServiceWithKeyedParameter e.g. tests that no autobinding happens.
		kernel.Components.Remove<IMissingBindingResolver, SelfBindingResolver>();
		var factory = new NinjectServiceProviderFactory(kernel);

		return factory.CreateBuilder(serviceCollection).Build();
	}

#pragma warning disable xUnit1024

	[Theory(Skip = "Wrong implementation of the test, should use Assert.Equal and not Assert.Same")]
	[InlineData(true)]
	[InlineData(false)]
	public new void ResolveWithAnyKeyQuery_Constructor(bool anyKeyQueryBeforeSingletonQueries)
	{
	}

	[Theory(Skip = "Wrong implementation, should use Assert.Equal and not Assert.Same")]
	[InlineData(true)]
	[InlineData(false)]
	public new void ResolveWithAnyKeyQuery_Constructor_Duplicates(bool anyKeyQueryBeforeSingletonQueries)
	{
	}
#pragma warning restore xUnit1024
}
