using FluentAssertions;
using Ninject.Web.AspNetCore.Components;
using System;
using System.Collections.Generic;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public  class GenericConstraintValidtorTest
	{
		private readonly ConstrainedGenericBindingResolver _resolver = new ConstrainedGenericBindingResolver();

		[Fact]
		public void SimpleConstrainedGeneric_AgainstNonGenericDefinition_ThrowsArgumentException()
		{
			Action action = () => _resolver.SatisfiesGenericTypeConstraints(typeof(SimpleConstrainedType<InvalidCastException>), typeof(SimpleConstrainedType<InvalidCastException>));

			action.Should().Throw<ArgumentException>();
		}

		[Fact]
		public void SimpleConstrainedGeneric_WithValidTypes_Successful()
		{
			var result = _resolver.SatisfiesGenericTypeConstraints(typeof(SimpleConstrainedType<InvalidCastException>), typeof(SimpleConstrainedType<>));

			result.Should().BeTrue();
		}

		[Fact]
		public void SimpleConstrainedGeneric_WithNonMatchingType_Fails()
		{
			var result = _resolver.SatisfiesGenericTypeConstraints(typeof(List<InvalidCastException>), typeof(SimpleConstrainedType<>));

			result.Should().BeFalse();
		}

		[Fact]
		public void AdvancedConstrainedGeneric_WithValidTypes_Successful()
		{
			var result = _resolver.SatisfiesGenericTypeConstraints(typeof(AdvancedConstrainedType<IList<Uri>, Uri>), typeof(AdvancedConstrainedType<,>));

			result.Should().BeTrue();
		}

		[Fact]
		public void AdvancedConstrainedGeneric_WithNonMatchingType_Fails()
		{
			var result = _resolver.SatisfiesGenericTypeConstraints(typeof(List<InvalidCastException>), typeof(AdvancedConstrainedType<,>));

			result.Should().BeFalse();
		}

		private class SimpleConstrainedType<T>
			where T : Exception
		{
		}

		private class AdvancedConstrainedType<TContainer, T>
			where TContainer : IEnumerable<T>
			where T : class
		{
		}
	}
}
