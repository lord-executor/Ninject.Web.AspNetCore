using System;

namespace Ninject.Web.AspNetCore
{
	/// <summary>
	/// We have to wrap IKernel here as an IServiceProvider,
	/// although Ninject would itself implement this interface (but with a wrong semantic for the not found case).
	///
	/// Note: Although ASP.NET Core wants to use a method from ISupportRequiredService
	/// we don't need to implement it as there is an extension method based on IServiceProvider.
	/// </summary>
	public class NinjectServiceProvider : IServiceProvider
	{
		private readonly IKernel _kernel;
		
		public NinjectServiceProvider(IKernel kernel)
		{
			_kernel = kernel;
		}

		public object GetService(Type serviceType)
		{
			// call TryGet as IServiceProvider.GetService must return null if not found.
			var result = _kernel.TryGet(serviceType);
			return result;
		}

	}
}
