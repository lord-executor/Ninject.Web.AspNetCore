namespace Ninject.Web.AspNetCore
{
	/// Each <see cref="Microsoft.Extensions.DependencyInjection.ServiceProvider"/> instance implicitly creates
	/// a scope that is effectively tied to the service provider engine, making scoped instances created this way
	/// functionally equivalent to singleton services. This "feature" fortunately does not seem to be _used_ by
	/// ASP.NET Core - only transient and singleton services are ever instantiated with the "root" service provider.
	///
	/// Instantiating scoped services as singletons can be _very_ problematic since the assumption that the scope
	/// of the service is limited (usually to a single request) is violated which can potentially break a great
	/// many things.
	/// 
	/// This is why the _default_ behavior for the Ninject integration is to throw an exception with <see cref="Throw"/>,
	/// but if this should be necessary, the configuration can be adjusted to create the same behavior as the
	/// standard service provider implementation by using <see cref="UseKernel" /> - using the kernel as the scope
	/// is the Ninject equivalent of the implicit root scope.
	/// 
	/// It is also possible to use <see cref="UseTransient" /> to make sure that scoped services without a scope are
	/// created as transient services instead.
	public enum MissingRequestScopeBehaviorType
	{
		Throw,
		UseKernel,
		UseTransient,
	}
}
