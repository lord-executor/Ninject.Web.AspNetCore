using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore
{
	/// <summary>
	/// This interface allows to handle the differences between keyed and non keyed implementation instruction
	/// on ServiceDescriptors
	/// </summary>
	public interface IDescriptorAdapter
	{
		/// <summary>
		/// Returns the type to instantiate if instantiation should be done by type.
		/// </summary>
		Type ImplementationType { get; }

		/// <summary>
		/// Returns the instance if a specific instance is configured on the descriptor
		/// </summary>
		object ImplementationInstance { get; }

		/// <summary>
		/// Returns true, if a service factory is configured on the descriptor
		/// </summary>
		bool UseServiceFactory { get; }

		/// <summary>
		/// If UseServiceFactory returns true, use this method to instantiate via factory.
		/// </summary>
		object InstantiateFromServiceFactory(IServiceProvider provider);

		/// <summary>
		/// The lifetime coonfigured for the service descriptor
		/// </summary>
		ServiceLifetime  Lifetime { get; }
	}
}