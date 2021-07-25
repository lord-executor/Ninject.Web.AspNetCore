namespace Ninject.Web.AspNetCore
{
	public static class NinjectSettingsExtensions
	{
		public static MissingRequestScopeBehaviorType GetMissingRequestScopeBehavior(this INinjectSettings settings)
		{
			return settings.Get(nameof(MissingRequestScopeBehaviorType), MissingRequestScopeBehaviorType.Throw);
		}

		public static void SetMissingRequestScopeBehavior(this INinjectSettings settings, MissingRequestScopeBehaviorType behavior)
		{
			// TODO: review if this is still needed since ServiceProviderScopeParameter should apply to all services
			// registered with the service collection (see AspNetCoreApplicationPlugin)
			settings.Set(nameof(MissingRequestScopeBehaviorType), behavior);
		}
	}
}
