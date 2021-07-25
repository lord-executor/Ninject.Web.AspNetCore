using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ninject.Activation;
using Ninject.Activation.Caching;
using Ninject.Components;
using Ninject.Infrastructure;
using Ninject.Infrastructure.Disposal;
using Ninject.Planning.Bindings;

namespace Ninject.Web.AspNetCore
{
	public class OrderedCache : NinjectComponent, ICache
	{
		/// <summary>
		/// Contains all cached instances.
		/// This is a dictionary of scopes to a multimap for bindings to cache entries.
		/// </summary>
		private readonly IDictionary<object, Multimap<IBindingConfiguration, CacheEntry>> entries =
			new Dictionary<object, Multimap<IBindingConfiguration, CacheEntry>>(new WeakReferenceEqualityComparer());

		private readonly LinkedList<CacheEntry> orderedEntries = new LinkedList<CacheEntry>();

		/// <summary>
		/// Initializes a new instance of the <see cref="Cache"/> class.
		/// </summary>
		/// <param name="pipeline">The pipeline component.</param>
		/// <param name="cachePruner">The cache pruner component.</param>
		public OrderedCache(IPipeline pipeline, ICachePruner cachePruner)
		{
			Pipeline = pipeline;
			cachePruner.Start(this);
		}

		/// <summary>
		/// Gets the pipeline component.
		/// </summary>
		public IPipeline Pipeline { get; private set; }

		/// <summary>
		/// Gets the number of entries currently stored in the cache.
		/// </summary>
		public int Count
		{
			get { return GetAllCacheEntries().Count(); }
		}

		/// <summary>
		/// Releases resources held by the object.
		/// </summary>
		/// <param name="disposing"><c>True</c> if called manually, otherwise by GC.</param>
		public override void Dispose(bool disposing)
		{
			if (disposing && !IsDisposed)
			{
				Clear();
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Stores the specified context in the cache.
		/// </summary>
		/// <param name="context">The context to store.</param>
		/// <param name="reference">The instance reference.</param>
		public void Remember(IContext context, InstanceReference reference)
		{
			var scope = context.GetScope();
			var entry = new CacheEntry(context, reference);

			lock (entries)
			{
				var weakScopeReference = new ReferenceEqualWeakReference(scope);
				if (!entries.ContainsKey(weakScopeReference))
				{
					entries[weakScopeReference] = new Multimap<IBindingConfiguration, CacheEntry>();
					if (scope is INotifyWhenDisposed notifyScope)
					{
						notifyScope.Disposed += (o, e) => Clear(weakScopeReference);
					}
				}

				entries[weakScopeReference].Add(context.Binding.BindingConfiguration, entry);
				orderedEntries.AddLast(entry);
			}
		}

		/// <summary>
		/// Tries to retrieve an instance to re-use in the specified context.
		/// </summary>
		/// <param name="context">The context that is being activated.</param>
		/// <returns>The instance for re-use, or <see langword="null"/> if none has been stored.</returns>
		public object TryGet(IContext context)
		{
			var scope = context.GetScope();
			if (scope == null)
			{
				return null;
			}

			lock (entries)
			{
				if (!entries.TryGetValue(scope, out Multimap<IBindingConfiguration, CacheEntry> bindings))
				{
					return null;
				}

				foreach (var entry in bindings[context.Binding.BindingConfiguration])
				{
					if (context.HasInferredGenericArguments)
					{
						var cachedArguments = entry.Context.GenericArguments;
						var arguments = context.GenericArguments;

						if (!cachedArguments.SequenceEqual(arguments))
						{
							continue;
						}
					}

					return entry.Reference.Instance;
				}

				return null;
			}
		}

		/// <summary>
		/// Deactivates and releases the specified instance from the cache.
		/// </summary>
		/// <param name="instance">The instance to release.</param>
		/// <returns><see langword="True"/> if the instance was found and released; otherwise <see langword="false"/>.</returns>
		public bool Release(object instance)
		{
			lock (entries)
			{
				var instanceFound = false;
				using (var collector = Collector.Create(Pipeline, orderedEntries))
				{
					foreach (var bindingEntry in entries.Values.SelectMany(bindingEntries => bindingEntries.Values).ToList())
					{
						var instanceEntries = bindingEntry.Where(cacheEntry => ReferenceEquals(instance, cacheEntry.Reference.Instance)).ToList();
						foreach (var cacheEntry in instanceEntries)
						{
							Forget(collector, cacheEntry);
							bindingEntry.Remove(cacheEntry);
							instanceFound = true;
						}
					}
				}

				return instanceFound;
			}
		}

		/// <summary>
		/// Removes instances from the cache which should no longer be re-used.
		/// </summary>
		public void Prune()
		{
			lock (entries)
			{
				using (var collector = Collector.Create(Pipeline, orderedEntries))
				{
					var disposedScopes = entries.Where(scope => !((ReferenceEqualWeakReference)scope.Key).IsAlive).Select(scope => scope).ToList();
					foreach (var disposedScope in disposedScopes)
					{
						entries.Remove(disposedScope.Key);
						Forget(collector, GetAllBindingEntries(disposedScope.Value));
					}
				}
			}
		}

		/// <summary>
		/// Immediately deactivates and removes all instances in the cache that are owned by
		/// the specified scope.
		/// </summary>
		/// <param name="scope">The scope whose instances should be deactivated.</param>
		public void Clear(object scope)
		{
			lock (entries)
			{
				using (var collector = Collector.Create(Pipeline, orderedEntries))
				{
					if (entries.TryGetValue(scope, out Multimap<IBindingConfiguration, CacheEntry> bindings))
					{
						entries.Remove(scope);
						Forget(collector, GetAllBindingEntries(bindings));
					}
				}
			}
		}

		/// <summary>
		/// Immediately deactivates and removes all instances in the cache, regardless of scope.
		/// </summary>
		public void Clear()
		{
			lock (entries)
			{
				using (var collector = Collector.Create(Pipeline, orderedEntries))
				{
					Forget(collector, GetAllCacheEntries());
					entries.Clear();
				}
			}
		}

		/// <summary>
		/// Gets all entries for a binding within the selected scope.
		/// </summary>
		/// <param name="bindings">The bindings.</param>
		/// <returns>All bindings of a binding.</returns>
		private static IEnumerable<CacheEntry> GetAllBindingEntries(Multimap<IBindingConfiguration, CacheEntry> bindings)
		{
			return bindings.Values.SelectMany(bindingEntries => bindingEntries);
		}

		/// <summary>
		/// Gets all cache entries.
		/// </summary>
		/// <returns>Returns all cache entries.</returns>
		private IEnumerable<CacheEntry> GetAllCacheEntries()
		{
			return entries.SelectMany(scopeCache => GetAllBindingEntries(scopeCache.Value));
		}

		/// <summary>
		/// Forgets the specified cache entries.
		/// </summary>
		/// <param name="cacheEntries">The cache entries.</param>
		private void Forget(Collector collector, IEnumerable<CacheEntry> cacheEntries)
		{
			foreach (var entry in cacheEntries.ToList())
			{
				Forget(collector, entry);
			}
		}

		/// <summary>
		/// Forgets the specified entry.
		/// </summary>
		/// <param name="entry">The entry.</param>
		private void Forget(Collector collector, CacheEntry entry)
		{
			Clear(entry.Reference.Instance);
			collector.Register(entry);
		}

		/// <summary>
		/// An entry in the cache.
		/// </summary>
		private class CacheEntry
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="CacheEntry"/> class.
			/// </summary>
			/// <param name="context">The context.</param>
			/// <param name="reference">The instance reference.</param>
			public CacheEntry(IContext context, InstanceReference reference)
			{
				Context = context;
				Reference = reference;
			}

			/// <summary>
			/// Gets the context of the instance.
			/// </summary>
			/// <value>The context.</value>
			public IContext Context { get; private set; }

			/// <summary>
			/// Gets the instance reference.
			/// </summary>
			/// <value>The instance reference.</value>
			public InstanceReference Reference { get; private set; }
		}

		private class Collector : IDisposable
		{
			private static AsyncLocal<Collector> _current = new AsyncLocal<Collector>();

			private readonly Collector _root;
			private readonly IPipeline _pipeline;
			private readonly LinkedList<CacheEntry> _entries;
			private readonly HashSet<CacheEntry> _deletes = new HashSet<CacheEntry>();

			public static Collector Create(IPipeline pipeline, LinkedList<CacheEntry> entries)
			{
				if (_current.Value == null)
				{
					return _current.Value = new Collector(pipeline, entries);
				}
				else
				{
					return new Collector(_current.Value);
				}
			}

			private Collector(IPipeline pipeline, LinkedList<CacheEntry> entries)
			{
				_pipeline = pipeline;
				_entries = entries;
			}

			private Collector(Collector root)
			{
				_root = root;
			}

			public void Register(CacheEntry entry)
			{
				if (_root == null)
				{
					_deletes.Add(entry);
				}
				else
				{
					_root.Register(entry);
				}
			}

			public void Dispose()
			{
				if (_root == null)
				{
					var node = _entries.Last;
					if (node == null)
					{
						return;
					}

					do
					{
						var current = node;
						node = node.Previous;
						if (_deletes.Contains(current.Value))
						{
							_entries.Remove(current);
							_pipeline.Deactivate(current.Value.Context, current.Value.Reference);
						}
					} while (node != null);

					_current.Value = null;
				}
			}
		}
	}
}
