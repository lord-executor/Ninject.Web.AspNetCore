using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore.ComplianceTest;

/// <summary>
/// Extension of the Microsoft DI specification tests for implementations of IKeyedServiceProvider
/// </summary>
public class KeyedDependencyInjectionComplianceTest : Microsoft.Extensions.DependencyInjection.Specification.KeyedDependencyInjectionSpecificationTests
{
	protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
	{
		var kernel = new AspNetCoreKernel();
		var factory = new NinjectServiceProviderFactory(kernel);

		return factory.CreateBuilder(serviceCollection).Build();
	}
}