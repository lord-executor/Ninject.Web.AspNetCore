using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.WebApiCore
{
	public class NInjectServiceScopeFactory : IServiceScopeFactory
	{

		private readonly IKernel _kernel;
		public NInjectServiceScopeFactory(IKernel kernel)
		{
			_kernel = kernel;
		}

		public IServiceScope CreateScope()
		{
			return new NInjectServiceScope(_kernel);
		}
	}
}
