using Ninject.Infrastructure;
using System.Threading;

namespace Ninject.Web.AspNetCore.Components
{
	public class ActivationEntry : IActivationEntry
	{
		private static long _counter = 0;

		public ReferenceEqualWeakReference Reference { get; }
		public long Order { get; }
		public bool IsDeactivated { get; set; }

		public ActivationEntry(object instance)
		{
			Reference = new ReferenceEqualWeakReference(instance);
			// Based on the same reasoning as DateTime.Ticks, even in a scenario where an application
			// were to create 10 million (10^7) service instances _per second_ consistently and
			// uninterrupted, the long would still last roughly 29'227 years before overflowing.
			// Soooo.... this should be safe enough to guarantee the correct disposal order.
			Order = Interlocked.Increment(ref _counter);
		}
	}
}
