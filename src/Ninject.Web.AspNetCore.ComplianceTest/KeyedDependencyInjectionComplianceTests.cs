using System;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Planning.Bindings.Resolvers;

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
}
