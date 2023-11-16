using System.Linq;
using Ninject.Activation;

namespace Ninject.Web.AspNetCore
{
	/// <summary>
	/// Extension methods on <see cref="IContext"/> to simplify dealing with the <see cref="ServiceProviderScopeParameter" />
	/// </summary>
	public static class ContextExtensions
	{
		/// <summary>
		/// Gets the <see cref="ServiceProviderScopeParameter"/> from the context parameters.
		/// </summary>
		/// <param name="context">The resolution context from which to retrieve the scope parameter</param>
		/// <returns>The scope from which the request originated or <c>null</c> if there is none</returns>
		public static ServiceProviderScopeParameter GetServiceProviderScopeParameter(this IContext context)
		{
			return context.Parameters.OfType<ServiceProviderScopeParameter>().SingleOrDefault();
		}
	}
}