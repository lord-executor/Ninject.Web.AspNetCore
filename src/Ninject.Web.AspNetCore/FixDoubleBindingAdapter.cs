using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore
{
	/// <summary>
	/// This class fixes the issue that ASP.NET core registers 2 implementations for IServer and tries to inject IServer then with single injection
	/// This resolves to the last registered service in ASP.NET core whereas in a reasonable injection container, it would lead to an exception
	/// as there is no way to decide which one would be the right binding to use.
	/// </summary>
	public class FixDoubleBindingAdapter : IPopulateAdapter
	{
		public void AdaptAfterPopulate(IKernel kernel)
		{
			// nothing to do here.
		}

		public bool AdaptDescriptor(IKernel kernel, ServiceDescriptor serviceDescriptor)
		{
			if (serviceDescriptor.ServiceType.FullName == "Microsoft.AspNetCore.Hosting.Server.IServer" && 
			    serviceDescriptor.ServiceType.Assembly.GetName().Name == "Microsoft.AspNetCore.Hosting.Server.Abstractions")
			{
				kernel.Rebind(serviceDescriptor.ServiceType).ConfigureImplementationAndLifecycle(serviceDescriptor);
				return true;
			}

			return false;
		}
	}
}
