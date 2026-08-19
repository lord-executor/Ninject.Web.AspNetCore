using AwesomeAssertions;
using Ninject.Web.AspNetCore.Components;
using Ninject.Web.AspNetCore.Test.Fakes;
using System;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	/// <summary>
	/// Copy of the ActivationCacheTests from the Ninject core library to ensure that the replacement
	/// activation cache still behaves as Ninject expects it to.
	/// </summary>
	public sealed class ActivationCacheTests : IDisposable
	{
		private readonly WeakTableActivationCache _testee;

		public ActivationCacheTests()
		{
			_testee = new WeakTableActivationCache();
		}

		[Fact]
		public void IsActivatedReturnsFalseForObjectsNotInTheActivationCache()
		{
			var activated = _testee.IsActivated(new object());

			activated.Should().BeFalse();
		}

		[Fact]
		public void IsActivatedReturnsTrueForObjectsInTheActivationCache()
		{
			var instance = new TestObject(42);

			_testee.AddActivatedInstance(instance);
			var activated = _testee.IsActivated(instance);
			var activatedObjectCount = _testee.ActivatedObjectCount;

			activated.Should().BeTrue();
			activatedObjectCount.Should().Be(1);
		}

		[Fact]
		public void IsDeactivatedReturnsFalseForObjectsNotInTheDeactivationCache()
		{
			var activated = _testee.IsDeactivated(new object());

			activated.Should().BeFalse();
		}

		[Fact]
		public void IsDeactivatedReturnsTrueForObjectsInTheDeactivationCache()
		{
			var instance = new TestObject(42);

			_testee.AddDeactivatedInstance(instance);
			var deactivated = _testee.IsDeactivated(instance);
			var deactivatedObjectCount = _testee.DeactivatedObjectCount;

			deactivated.Should().BeTrue();
			deactivatedObjectCount.Should().Be(1);
		}

		[Fact]
		public void DeadObjectsAreRemoved()
		{
			CreateCollectableInstances(_testee);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			_testee.Prune();

			var activatedObjectCount = _testee.ActivatedObjectCount;
			var deactivatedObjectCount = _testee.DeactivatedObjectCount;

			activatedObjectCount.Should().Be(0);
			deactivatedObjectCount.Should().Be(0);
		}

		[Fact]
		public void ImplementationDoesNotRelyOnObjectHashCode()
		{
			var instance = new TestObject(42);

			_testee.AddActivatedInstance(instance);
			instance.ChangeHashCode(43);
			var isActivated = _testee.IsActivated(instance);

			isActivated.Should().BeTrue();
		}

		/// <summary>
		/// Depending on the optimization level (tiered optimization), local variables created inside of a method
		/// are "untracked" and can't get collected during the execution of that method even if no variable
		/// currently references it. This is why we have to create our objects in a separate method for .NET 5 so
		/// that we can check if they are collected in the main test method.
		/// 
		/// See:
		/// * https://stackoverflow.com/questions/67115842/why-net-5-gc-doesnt-collect-or-at-least-calling-finalize-clearly-dereference
		/// * https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-5/
		/// * https://github.com/dotnet/runtime/blob/9900dfb4b2e32cf02ca846adaf11e93211629ede/docs/design/features/tiered-compilation.md
		/// </summary>
		/// <param name="activationCache"></param>
		private void CreateCollectableInstances(WeakTableActivationCache activationCache)
		{
			activationCache.AddActivatedInstance(new TestObject(42));
			activationCache.AddDeactivatedInstance(new TestObject(42));
		}

		public void Dispose()
		{
			_testee?.Dispose();
		}
	}
}