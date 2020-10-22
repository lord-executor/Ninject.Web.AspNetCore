using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.WebApiCore
{
	/// <summary>
	/// Allows to adapt the population form ServiceCollection process.
	/// </summary>
	public interface IPopulateAdapter
	{

		void AdaptAfterPopulate(IKernel kernel);

		/// <summary>
		/// Allow to customize the binding conversion for certain bindings.
		/// If true is returned, the default conversion is not applied; in case of false it is.
		/// </summary>
		bool AdaptDescriptor(IKernel kernel, ServiceDescriptor serviceDescriptor);

	}
}
