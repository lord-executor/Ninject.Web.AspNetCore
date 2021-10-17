using Ninject.Infrastructure;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ninject.Web.AspNetCore.Components
{
	public class UnwrappingReferenceEqualityComparer : IEqualityComparer<object>
	{
		public new bool Equals(object x, object y)
		{
			// micro-optimization: we are checking "y" first, because we know that in DisposalManager, x will be the "plain" object
			// and y will be the weak reference.
			return y is ReferenceEqualWeakReference weakY && weakY.Equals(x) || x is ReferenceEqualWeakReference weakX && weakX.Equals(y);
		}

		public int GetHashCode(object obj)
		{
			var weakReference = obj as ReferenceEqualWeakReference;
			return weakReference?.GetHashCode() ?? RuntimeHelpers.GetHashCode(obj);
		}
	}
}
