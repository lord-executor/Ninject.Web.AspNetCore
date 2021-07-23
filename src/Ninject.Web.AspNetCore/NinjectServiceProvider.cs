using Microsoft.Extensions.DependencyInjection;
using Ninject.Syntax;
using System;

namespace Ninject.Web.AspNetCore
{
	/// <summary>
	/// We have to wrap IKernel here as an IServiceProvider,
	/// although Ninject would itself implement this interface (but with a wrong semantic for the not found case) 
	/// and additionally we want to implement the optional ISupportRequiredService.
	/// 
	/// Note: ASP.NET Core wants to use a method from ISupportRequiredService to resolve a non-optional service.
	/// Although it's implemented on Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions in a generic way
	/// we implement it here to have the nicer exceptions from NInject so that it's possible to distinguish the "not registered at all"
	/// vs the "ambigious matches found" cases.
	/// </summary>
	public class NinjectServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable
	{
		private readonly IResolutionRoot _resolutionRoot;

		public NinjectServiceProvider(IResolutionRoot resolutionRoot)
		{
			_resolutionRoot = resolutionRoot;
		}

		public object GetRequiredService(Type serviceType)
		{
			var result = _resolutionRoot.Get(serviceType);
			return result;
		}

		public object GetService(Type serviceType)
		{
			var result = _resolutionRoot.TryGet(serviceType);
			return result;
		}

		public void Dispose()
		{
			(_resolutionRoot as IDisposable)?.Dispose();
		}
	}
}
