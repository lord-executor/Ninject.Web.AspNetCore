using FluentAssertions;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Ninject.Web.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class IndexedBindingPrecedenceComparerTest
	{
		[Fact]
		public void BindingPrecedence_ImplicitVsExplicit_ExplicitWins()
		{
			var comparer = new IndexedBindingPrecedenceComparer();
			var bindings = BindingVariants().ToList();

			for (var i = 0; i < bindings.Count; i++)
			{
				for (var j = 0; j < bindings.Count; j++)
				{
					if (i < j)
					{
						comparer.Compare(bindings[i], bindings[j]).Should().BeGreaterThan(0);
					}
					else if (i > j)
					{
						comparer.Compare(bindings[i], bindings[j]).Should().BeLessThan(0);
					}
					else
					{
						comparer.Compare(bindings[i], bindings[j]).Should().Be(0);
					}
				}
			}
		}

		[Fact]
		public void BindingPrecedence_WithDecidingIndex_HighestIndexWins()
		{
			var index = new BindingIndex();
			var first = new DummyBinding().WithIndex(index);
			var second = new DummyBinding().WithIndex(index);
			var noIndex = new DummyBinding();
			var comparer = new IndexedBindingPrecedenceComparer();

			comparer.Compare(first, second).Should().BeGreaterThan(0); // first > second
			comparer.Compare(second, first).Should().BeLessThan(0); // second < first
			comparer.Compare(first, first).Should().Be(0); // first == first
			comparer.Compare(second, second).Should().Be(0); // second == second

			comparer.Compare(first, noIndex).Should().BeGreaterThan(0); // first > noIndex
			comparer.Compare(second, noIndex).Should().BeGreaterThan(0); // second > noIndex
			comparer.Compare(noIndex, noIndex).Should().Be(0); // noIndex == noIndex
		}

		private IEnumerable<IBinding> BindingVariants()
		{
			var index = new BindingIndex();

			yield return new DummyBinding().WithIndex(index);
			yield return new DummyBinding().WithIndex(index);
			yield return new DummyBinding().WithIndex(index);
			yield return new DummyBinding { IsConditional = true };
			yield return new DummyBinding() { Service = typeof(Predicate<string>) };
			yield return new DummyBinding() { Service = typeof(Predicate<>) };
			yield return new DummyBinding { Service = typeof(Predicate<>), IsImplicit = true };
		}

		private class DummyBinding : IBinding
		{
			public Type Service { get; set; }
			public bool IsImplicit { get; set; }
			public bool IsConditional { get; set; }
			public IBindingMetadata Metadata { get; } = new BindingMetadata();

			public DummyBinding()
			{
				Service = typeof(DummyBinding);
			}

			public DummyBinding WithIndex(BindingIndex index)
			{
				Metadata.Set(nameof(BindingIndex), index.Next(Service));
				return this;
			}

			public IBindingConfiguration BindingConfiguration => throw new NotImplementedException();
			public BindingTarget Target { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public Func<IRequest, bool> Condition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public Func<IContext, IProvider> ProviderCallback { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public Func<IContext, object> ScopeCallback { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
			public ICollection<IParameter> Parameters => throw new NotImplementedException();
			public ICollection<Action<IContext, object>> ActivationActions => throw new NotImplementedException();
			public ICollection<Action<IContext, object>> DeactivationActions => throw new NotImplementedException();

			public IProvider GetProvider(IContext context)
			{
				throw new NotImplementedException();
			}

			public object GetScope(IContext context)
			{
				throw new NotImplementedException();
			}

			public bool Matches(IRequest request)
			{
				throw new NotImplementedException();
			}
		}
	}
}
