using Ninject.Activation.Caching;
using Ninject.Web.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ninject.Web.AspNetCore.Test.Fakes
{
	public class FakeActivationCacheAccessor : IActivationCache, IActivationCacheAccessor
	{
		private readonly ConditionalWeakTable<object, ActivationEntry> _trackedInstances = new ConditionalWeakTable<object, ActivationEntry>();

		public FakeActivationCacheAccessor(IEnumerable<object> activatedInstances)
		{
			foreach (var obj in activatedInstances)
			{
				_trackedInstances.Add(obj, new ActivationEntry(obj));
			}
		}

		public INinjectSettings Settings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public void AddActivatedInstance(object instance)
		{
			throw new NotImplementedException();
		}

		public void AddDeactivatedInstance(object instance)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IActivationEntry> GetAllEntries()
		{
			return _trackedInstances.Select(kvp => kvp.Value);
		}

		public IActivationEntry GetEntry(object instance)
		{
			return _trackedInstances.TryGetValue(instance, out var entry) ? entry : null;
		}

		public bool IsActivated(object instance)
		{
			throw new NotImplementedException();
		}

		public bool IsDeactivated(object instance)
		{
			throw new NotImplementedException();
		}
	}
}
