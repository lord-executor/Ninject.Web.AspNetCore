using Ninject.Activation;
using Ninject.Components;
using Ninject.Infrastructure;
using System;
using System.Collections.Generic;

namespace Ninject.Web.AspNetCore
{
	// TODO: move to "Components" namespace along with other custom Ninject components
	public class DisposalManager : NinjectComponent, IDisposalManager
	{
		private readonly LinkedList<ReferenceEqualWeakReference> activeInstances = new LinkedList<ReferenceEqualWeakReference>();
		private IDisposalCollectorArea _area;

		public void AddInstance(InstanceReference instanceReference)
		{
			activeInstances.AddLast(new ReferenceEqualWeakReference(instanceReference.Instance));
		}

		public IDisposalCollectorArea CreateArea()
		{
			if (_area == null)
			{
				return _area = new OrderedAggregateDisposalArea(this);
			}

			return new InnerDisposalArea(_area);
		}

		public void RemoveInstance(InstanceReference instanceReference)
		{
			(_area as IDisposalCollector ?? ImmediateDisposal.Instance).Register(instanceReference);
		}


		private class ImmediateDisposal : IDisposalCollector
		{
			public static ImmediateDisposal Instance { get; } = new ImmediateDisposal();

			public void Register(InstanceReference instanceReference)
			{
				(instanceReference.Instance as IDisposable)?.Dispose();
			}
		}

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
	}
}
