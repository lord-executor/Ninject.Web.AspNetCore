using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore
{
	public class NinjectServiceScope : IServiceScope
	{

		private bool _disposed;
		private readonly IKernel _kernel;
		private readonly RequestScope _scope;

		public NinjectServiceScope(IKernel kernel)
		{
			_kernel = kernel;
			if (RequestScope.Current == null)
			{
				_scope = new RequestScope();
			}

			ServiceProvider = new NinjectServiceProvider(new ServiceProviderScopeResolutionRoot(_kernel));
		}

		// note that we can't return the IKernel directly here, although it would implement IServiceProvider.
		// the problem is, that Ninject incorrectly throws an exception if no binding resolvable whereas
		// IServiceProvider.GetService requires to return null in this case.
		// see https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider.getservice?view=netcore-3.1
		public IServiceProvider ServiceProvider { get; }

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this._disposed)
			{
				this._disposed = true;

				if (disposing && _scope != null)
				{
					_scope.Dispose();
				}
			}
		}

	}
}
