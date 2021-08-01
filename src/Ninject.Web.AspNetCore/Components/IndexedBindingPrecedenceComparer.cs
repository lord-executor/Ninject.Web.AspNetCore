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

			// the 4 criteria are ordered from least, to most significant
			var ninjectPrecedence = new List<Func<IBinding, bool>>
							{
								b => !b.IsImplicit,   // explicit bindings > implicit
								b => !b.Service.ContainsGenericParameters, // closed generics > open generics
								b => b.IsConditional, // conditional bindings > unconditional
								b => b != null,       // null bindings should never happen, but just in case
                            };
			var indexDifference = GetBindingIndexPrecedence(x) - GetBindingIndexPrecedence(y);

			// xPrecedence and yPrecedence both are 5 bit numbers made up from the 4 bits from ninjectPrecedence and the 5th (hightest) bit being made up by the normalized
			// binding index difference. The two 5 bit numbers are then compared and the larger one wins.
			var xPrecedence = ninjectPrecedence
				.Select((f, index) => f(x) ? 1U << index : 0U)
				.Append(indexDifference > 0 ? 1U << 4 : 0U)
				.Aggregate(0L, (acc, val) => acc |= val);
			var yPrecedence = ninjectPrecedence
				.Select((f, index) => f(y) ? 1U << index : 0U)
				.Append(indexDifference < 0 ? 1U << 4 : 0U)
				.Aggregate(0L, (acc, val) => acc |= val);
			return (int)(xPrecedence - yPrecedence);
		}

		private int GetBindingIndexPrecedence(IBinding binding)
		{
			return (int)(binding.Metadata.Has(nameof(BindingIndex)) ? binding.Metadata.Get<BindingIndex.Item>(nameof(BindingIndex))?.Precedence : 0);
		}
	}
}
