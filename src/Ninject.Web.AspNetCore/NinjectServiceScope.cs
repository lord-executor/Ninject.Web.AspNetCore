using System;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Infrastructure.Disposal;

namespace Ninject.Web.AspNetCore
{
	public class NinjectServiceScope : DisposableObject, IServiceScope
	{
		public NinjectServiceScope(IKernel kernel, bool isRootScope)
		{
			var resolutionRoot = new ServiceProviderScopeResolutionRoot(kernel, this);

			ServiceProvider = new NinjectServiceProvider(resolutionRoot, isRootScope ? this : null);
		}

		// Note that we can't return the IKernel directly here, although it would implement IServiceProvider.
		// The problem is, that Ninject incorrectly throws an exception if no binding resolvable whereas
		// IServiceProvider.GetService requires to return null in this case.
		// see https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider.getservice?view=netcore-3.1.
		// Additionally, emulating the scope mechanism of IServiceProvider also requires a different resolution
		// root implementation.
		public IServiceProvider ServiceProvider { get; }
	}
}
