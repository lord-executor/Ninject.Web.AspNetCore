using Ninject.Activation;
using Ninject.Activation.Strategies;
using Ninject.Components;

namespace Ninject.Web.AspNetCore.Components
{
	/// <summary>
	/// Replaces the default <see cref="Ninject.Activation.Strategies.DisposableStrategy"/> to integrate the <see cref="IDisposalManager"/>
	/// mechanism for deterministic disposal order.
	/// </summary>
	public class OrderedDisposalStrategy : NinjectComponent, IActivationStrategy
	{
		public IDisposalManager DisposalManager { get; set; }

		public OrderedDisposalStrategy(IDisposalManager disposalManager)
		{
			DisposalManager = disposalManager;
		}

		public void Activate(IContext context, InstanceReference reference)
		{
			DisposalManager.AddInstance(reference);
		}

		public void Deactivate(IContext context, InstanceReference reference)
		{
			DisposalManager.RemoveInstance(reference);
		}
	}
}
