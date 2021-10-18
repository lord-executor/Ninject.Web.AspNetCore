using Ninject.Activation;
using Ninject.Activation.Caching;
using Ninject.Components;
using Ninject.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

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
		private readonly ListNode _head = new ListNode(null);
		private readonly AsyncLocal<IDisposalCollectorArea> _area = new AsyncLocal<IDisposalCollectorArea>();
		private readonly object _disposalLock = new object();

		public void AddInstance(InstanceReference instanceReference)
		{
			lock (_head)
			{
				_head.Insert(new ListNode(new ReferenceEqualWeakReference(instanceReference.Instance)));
			}
		}

		public void RemoveInstance(InstanceReference instanceReference)
		{
			(_area.Value as IDisposalCollector ?? ImmediateDisposal.Instance).Register(instanceReference);
		}

		/// <summary>
		/// This is meant as a debugging tool to allow an application to analyze which services are remaining
		/// active that maybe should not be. In particular in high-load scenarios where many services are created
		/// for each request, this can be useful to analze scoping issues.
		/// </summary>
		/// <returns>The list of all currently tracked references - note, that any weak reference can become "dead"
		/// at any poing in time.</returns>
		public IList<ReferenceEqualWeakReference> GetActiveReferences()
		{
			return _head.DebugList;
		}

		public IDisposalCollectorArea CreateArea()
		{
			if (_area.Value == null)
			{
				return _area.Value = new OrderedAggregateDisposalArea(this);
			}

			return new InnerDisposalArea(_area.Value);
		}

		/// <summary>
		/// To help with concurrency of creating and disposing services in a multi-threaded environment, we use a single-lined
		/// "queue" with a dedicated head that does not hold a value where we can add items. This means that we can still add
		/// new items to the queue while we are also cleaning up the tail of the queue since we only have to lock the head
		/// while removing items directly following the head.
		/// </summary>
		[DebuggerDisplay("{DebugList}")]
		private class ListNode : IEnumerable<ReferenceEqualWeakReference>
		{
			public ReferenceEqualWeakReference Value { get; }

			public ListNode Next { get; set; }

			// only used for debugging
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public IList<ReferenceEqualWeakReference> DebugList => new List<ReferenceEqualWeakReference>(this);

			public ListNode(ReferenceEqualWeakReference value)
			{
				Value = value;
			}

			public void Insert(ListNode node)
			{
				node.Next = Next;
				Next = node;
			}

			/// <summary>
			/// This is a (hopefully) clever algorithm that removes all nodes _directly following_ the current one
			/// that are either already dead or need to be disposed. At the end, <see cref="Next"/> will point to
			/// the next queue entry that is still alive and does not need to be disposed.
			/// </summary>
			public ListNode PruneAndDispose(Func<ReferenceEqualWeakReference, bool> disposalCondition)
			{
				if (Next == null)
				{
					// nothing to do here by definition
					return null;
				}

				var current = Next;

				var needsDisposal = disposalCondition(current.Value);
				// It is always possible that a service went out of scope that wasn't managed by an IServiceScope,
				// so we also prune dead service references from the queue here.
				while (needsDisposal || !current.Value.IsAlive)
				{
					if (needsDisposal)
					{
						(current.Value.Target as IDisposable)?.Dispose();
					}
					current = current.Next;
					if (current == null)
					{
						// early exit if we have reached the end of the queue - everything from "this" to the end
						// of the very end of the queue can be removed.
						break;
					}
					needsDisposal = disposalCondition(current.Value);
				}

				if (Next != current)
				{
					Next = current;
				}

				return current;
			}

			public IEnumerator<ReferenceEqualWeakReference> GetEnumerator()
			{
				var current = this;
				while (current != null)
				{
					yield return current.Value;
					current = current.Next;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
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
			/// <summary>
			/// The disposal area should be a very limited scope (see <see cref="NinjectServiceScope.Dispose(bool)"/>)
			/// which should allow us to avoid having to create yet another weak reference for every service that is being
			/// disposed. At the end of the <see cref="Dispose"/>, all of the (hard) references that are held in the
			/// <see cref="_disposals"/> set go out of scope and have been disposed (if they are disposable). We can still
			/// find the correct entry in the hash set by using <see cref="UnwrappingReferenceEqualityComparer"/>
			/// </summary>
			private readonly HashSet<object> _disposals = new HashSet<object>(new UnwrappingReferenceEqualityComparer());

			public OrderedAggregateDisposalArea(DisposalManager manager)
			{
				_manager = manager;
			}

			public void Dispose()
{
				if (_disposals.Count == 0)
				{
					return;
				}

				lock (_manager._disposalLock)
				{
					ListNode current;

					// while we are operating on the _head, it must be locked because of potential concurrency with AddInstance,
					// but as soon as we have moved past the head, we only need to keep the tail lock.
					lock (_manager._head)
					{
						current = _manager._head.PruneAndDispose(weakReference => _disposals.Contains(weakReference));
					}

					while (current != null)
					{
						current = current.PruneAndDispose(weakReference => _disposals.Contains(weakReference));
					}
				}

				_manager._area.Value = null;
				_disposals.Clear();
			}

			public void Register(InstanceReference instanceReference)
			{
				_disposals.Add(instanceReference.Instance);
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
		/// and services are only disposed with the disposal of the outermost <see cref="OrderedAggregateDisposalArea"/>
		/// because only there do we know all the services that need to be disposed.
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
