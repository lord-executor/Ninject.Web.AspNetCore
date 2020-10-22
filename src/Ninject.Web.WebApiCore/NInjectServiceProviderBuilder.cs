using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.WebApiCore
{
	public class NInjectServiceProviderBuilder
	{
		private readonly IServiceCollection _services;
		private readonly IKernel _kernel;

		public NInjectServiceProviderBuilder(IKernel kernel, IServiceCollection services)
		{
			_kernel = kernel;
			_services = services;
		}

		public NInjectServiceProvider Build()
		{
			var result = new NInjectServiceProvider(_kernel);
			_kernel.Bind<IServiceProvider>().ToConstant(result); // needed for factory methods in IServiceCollection
			_kernel.Populate(_services);
			return result;
		}

	}
}
