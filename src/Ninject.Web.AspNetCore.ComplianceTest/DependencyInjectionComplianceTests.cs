using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ninject.Web.AspNetCore.ComplianceTest;

/// <summary>
/// See https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection.Specification.Tests/src - the dotnet/runtime
/// project which contains the dependency injection library code also contains a set of "compliance tests" that can be run against a potential alternative
/// implementation to check if it is compliant. This class here is doing just that.
/// 
/// The project also contains a separate test project that includes these compliance tests for a set of compliant third party DI implementations like
/// Autofac and Lightinject under https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection/tests/DI.External.Tests.
/// 
/// All of this is part of the https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.DependencyInjection/Microsoft.Extensions.DependencyInjection.sln
/// solution of dotnet/runtime.
/// </summary>
public class DependencyInjectionComplianceTest : Microsoft.Extensions.DependencyInjection.Specification.DependencyInjectionSpecificationTests
{
	protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
	{
		var kernel = new AspNetCoreKernel();
		var factory = new NinjectServiceProviderFactory(kernel);

		return factory.CreateBuilder(serviceCollection).Build();
	}
}