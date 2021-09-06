using Ninject.Activation;
using Ninject.Components;
using Ninject.Infrastructure;
using System;
using System.Collections.Generic;

namespace Ninject.Web.AspNetCore.Components
{
	/// <summary>
	/// The disposal manager helps to dispose service instances in a deterministic order when a <see cref="NinjectServiceScope"/> is
	/// disposed. Specifically, it disposes services in the reverse order in which they were instantiated because that what
	/// is expected by a compliant implementation for Microsoft.Extensions.DependencyInjection. For more details, see
	/// DependencyInjectionComplianceTests with the tests:
	/// * DisposesInReverseOrderOfCreation
	/// * DisposingScopeDisposesService
	/// 
	/// This is accomplished by tracking the active service instances in an ordered linked list through hooking into the
	/// service activation pipeline with <see cref="OrderedDisposalStrategy"/>.
	/// </summary>
	public class DisposalManager : NinjectComponent, IDisposalManager
	{
		private readonly LinkedList<ReferenceEqualWeakReference> activeInstances = new LinkedList<ReferenceEqualWeakReference>();
		private IDisposalCollectorArea _area;

		public void AddInstance(InstanceReference instanceReference)
		{
			activeInstances.AddLast(new ReferenceEqualWeakReference(instanceReference.Instance));
		}

		public void RemoveInstance(InstanceReference instanceReference)
		{
			(_area as IDisposalCollector ?? ImmediateDisposal.Instance).Register(instanceReference);
		}

		public IDisposalCollectorArea CreateArea()
		{
			if (_area == null)
			{
				return _area = new OrderedAggregateDisposalArea(this);
			}

			return new InnerDisposalArea(_area);
		}


		#region IDisposalCollectorArea Implementations

		/// <summary>
		/// This collector area essentially just "buffers" the disposal until the area itself is disposed at which point
		/// it goes through the active service instances in reverse and disposed all the services that are "marked"
		/// for disposal.
		/// </summary>
		private class OrderedAggregateDisposalArea : IDisposalCollectorArea
		{
			private readonly DisposalManager _manager;
			private readonly HashSet<ReferenceEqualWeakReference> _disposals = new HashSet<ReferenceEqualWeakReference>();

			public OrderedAggregateDisposalArea(DisposalManager manager)
			{
				_manager = manager;
			}

			public void Dispose()
			{
				var node = _manager.activeInstances.Last;

				if (node == null || _disposals.Count == 0)
				{
					return;
				}

				do
				{
					var current = node;
					node = node.Previous;
					if (_disposals.Contains(current.Value))
					{
						_manager.activeInstances.Remove(current);
						(current.Value.Target as IDisposable)?.Dispose();
					}
					else if (!current.Value.IsAlive)
					{
						// It is always possible that a service went out of scope that wasn't managed by an IServiceScope,
						// so we also prune dead service references from the list here.
						_manager.activeInstances.Remove(current);
					}
				} while (node != null);

				_manager._area = null;
			}

			public void Register(InstanceReference instanceReference)
			{
				_disposals.Add(new ReferenceEqualWeakReference(instanceReference.Instance));
			}
		}

		/// <summary>
		/// If there is no active disposal collection area, we just immediately dispose as Ninject does normally.
		/// </summary>
		private class ImmediateDisposal : IDisposalCollector
		{
			public static ImmediateDisposal Instance { get; } = new ImmediateDisposal();

			public void Register(InstanceReference instanceReference)
			{
				(instanceReference.Instance as IDisposable)?.Dispose();
			}
		}

		/// <summary>
		/// When disposal collection areas are nested, everything is delegated to the outermost area
		/// and services are only disposed with the disposal of the outermost <see cref="OrderedAggregateDisposalArea"/>.
		/// </summary>
		private class InnerDisposalArea : IDisposalCollectorArea
		{
			private readonly IDisposalCollectorArea _parent;

			public InnerDisposalArea(IDisposalCollectorArea parent)
			{
				_parent = parent;
			}

			public void Dispose()
			{
				// nothing
			}

			public void Register(InstanceReference instanceReference)
			{
				_parent.Register(instanceReference);
			}
		}

		#endregion
	}
}
