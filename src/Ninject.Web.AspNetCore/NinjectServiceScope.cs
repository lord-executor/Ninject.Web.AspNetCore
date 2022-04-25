using System;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Infrastructure.Disposal;
using Ninject.Web.AspNetCore.Components;

namespace Ninject.Web.AspNetCore
{
	public class NinjectServiceScope : DisposableObject, IServiceScope
	{
		private readonly IDisposalManager _disposalManager;

		public NinjectServiceScope(IKernel kernel, bool isRootScope)
		{
			_disposalManager = kernel.Components.Get<IDisposalManager>();
			var resolutionRoot = new ServiceProviderScopeResolutionRoot(kernel, this);
			ServiceProvider = new NinjectServiceProvider(resolutionRoot, isRootScope ? this : null);
		}

		/// <summary>
		/// Note that we can't return the IKernel directly here, although it would implement IServiceProvider.
		/// Emulating the scope mechanism of IServiceProvider also requires a different resolution root implementation.
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		public override void Dispose(bool disposing)
		{
			using (_disposalManager.CreateArea())
			{
				base.Dispose(disposing);
			}
		}
	}
}
