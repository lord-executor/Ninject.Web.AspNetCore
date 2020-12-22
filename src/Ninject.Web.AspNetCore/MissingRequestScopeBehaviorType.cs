namespace Ninject.Web.AspNetCore
{
	public enum MissingRequestScopeBehaviorType
	{
		Throw,
		UseKernel,
		UseTransient,
	}
}
