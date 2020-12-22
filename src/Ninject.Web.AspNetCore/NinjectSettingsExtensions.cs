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
			settings.Set(nameof(MissingRequestScopeBehaviorType), behavior);
		}
	}
}
