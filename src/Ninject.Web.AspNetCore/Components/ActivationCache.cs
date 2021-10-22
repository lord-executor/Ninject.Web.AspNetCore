using System;
using System.Collections.Generic;
using Ninject.Activation.Caching;
using Ninject.Components;
using Ninject.Infrastructure;

namespace Ninject.Web.AspNetCore.Components
{
	/// <summary>
	/// Stores the objects that were activated
	/// </summary>
	public class ActivationCache : NinjectComponent, IActivationCache, IPruneable
	{
		/// <summary>
		/// The objects that were activated as reference equal weak references.
		/// </summary>
		private readonly HashSet<object> activatedObjects = new HashSet<object>(new WeakReferenceEqualityComparer());

		/// <summary>
		/// The objects that were activated as reference equal weak references.
		/// </summary>
		private readonly HashSet<object> deactivatedObjects = new HashSet<object>(new WeakReferenceEqualityComparer());

		/// <summary>
		/// Initializes a new instance of the <see cref="ActivationCache"/> class.
		/// </summary>
		/// <param name="cachePruner">The cache pruner.</param>
		public ActivationCache(ICachePruner cachePruner)
		{
			if (cachePruner == null)
			{
				throw new ArgumentNullException(nameof(cachePruner), "Cannot be null");
			}
			cachePruner.Start(this);
		}

		/// <summary>
		/// Gets the activated object count.
		/// </summary>
		/// <value>The activated object count.</value>
		public int ActivatedObjectCount
		{
			get
			{
				return activatedObjects.Count;
			}
		}

		/// <summary>
		/// Gets the deactivated object count.
		/// </summary>
		/// <value>The deactivated object count.</value>
		public int DeactivatedObjectCount
		{
			get
			{
				return deactivatedObjects.Count;
			}
		}

		/// <summary>
		/// Clears the cache.
		/// </summary>
		public void Clear()
		{
			lock (activatedObjects)
			{
				activatedObjects.Clear();
			}

			lock (deactivatedObjects)
			{
				deactivatedObjects.Clear();
			}
		}

		/// <summary>
		/// Adds an activated instance.
		/// </summary>
		/// <param name="instance">The instance to be added.</param>
		public void AddActivatedInstance(object instance)
		{
			lock (activatedObjects)
			{
				activatedObjects.Add(new ReferenceEqualWeakReference(instance));
			}
		}

		/// <summary>
		/// Adds an deactivated instance.
		/// </summary>
		/// <param name="instance">The instance to be added.</param>
		public void AddDeactivatedInstance(object instance)
		{
			lock (deactivatedObjects)
			{
				deactivatedObjects.Add(new ReferenceEqualWeakReference(instance));
			}
		}

		/// <summary>
		/// Determines whether the specified instance is activated.
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <returns>
		///     <c>true</c> if the specified instance is activated; otherwise, <c>false</c>.
		/// </returns>
		public bool IsActivated(object instance)
		{
			return activatedObjects.Contains(instance);
		}

		/// <summary>
		/// Determines whether the specified instance is deactivated.
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <returns>
		///     <c>true</c> if the specified instance is deactivated; otherwise, <c>false</c>.
		/// </returns>
		public bool IsDeactivated(object instance)
		{
			return deactivatedObjects.Contains(instance);
		}

		/// <summary>
		/// Prunes this instance.
		/// </summary>
		public void Prune()
		{
			lock (activatedObjects)
			{
				RemoveDeadObjects(activatedObjects);
			}

			lock (deactivatedObjects)
			{
				RemoveDeadObjects(deactivatedObjects);
			}
		}

		/// <summary>
		/// Removes all dead objects.
		/// </summary>
		/// <param name="objects">The objects collection to be freed of dead objects.</param>
		private static void RemoveDeadObjects(HashSet<object> objects)
		{
			objects.RemoveWhere(reference => !((ReferenceEqualWeakReference)reference).IsAlive);
		}
	}
}