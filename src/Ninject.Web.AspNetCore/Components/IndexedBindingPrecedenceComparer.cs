using Ninject.Components;
using Ninject.Planning.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ninject.Web.AspNetCore.Components
{
	/// <summary>
	/// This is very similar to the <see cref="Ninject.Planning.Bindings.BindingPrecedenceComparer"/> except that it considers the
	/// <see cref="BindingIndex.Item.Precedence"/> as the top priority which is based on the order in which the binding was added.
	/// This is to satisfy the Microsoft.Extensions.DependencyInjection requirements that expect multi-injected service to be
	/// instantiated and injected in the order in which their service descriptors were added.
	/// See DependencyInjectionComplianceTests with the tests:
	/// * RegistrationOrderIsPreservedWhenServicesAreIEnumerableResolved
	/// </summary>
	public class IndexedBindingPrecedenceComparer : NinjectComponent, IBindingPrecedenceComparer
	{
		public int Compare(IBinding x, IBinding y)
		{
			if (x == y)
			{
				return 0;
			}

			// TODO: can be improved
			var funcs = new List<Func<IBinding, uint>>
							{
								b => (b.Metadata.Has(nameof(BindingIndex)) ? (uint)b.Metadata.Get<BindingIndex.Item>(nameof(BindingIndex))?.Precedence : 0) << 4,
								b => b != null ? 1U << 3 : 0,       // null bindings should never happen, but just in case
                                b => b.IsConditional ? 1U << 2 : 0, // conditional bindings > unconditional
                                b => !b.Service.ContainsGenericParameters ? 1U << 1 : 0, // closed generics > open generics
                                b => !b.IsImplicit ? 1U << 0 : 0,   // explicit bindings > implicit
                            };

			var xPrecedence = funcs.Select(f => f(x)).Aggregate(0L, (acc, val) => acc |= val);
			var yPrecedence = funcs.Select(f => f(y)).Aggregate(0L, (acc, val) => acc |= val);
			return (int)(xPrecedence - yPrecedence);
		}
	}
}
