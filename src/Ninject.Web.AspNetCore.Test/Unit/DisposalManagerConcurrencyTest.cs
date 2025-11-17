using AwesomeAssertions;
using Ninject.Web.AspNetCore.Components;
using Ninject.Web.AspNetCore.Test.Fakes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class DisposalManagerConcurrencyTest
	{
		[Fact]
		public async Task CreateArea_FromDifferentConcurrentThreads_ShouldCreateSeparateRootAreas()
		{
			var disposalManager = new DisposalManager(new FakeActivationCacheAccessor(Enumerable.Empty<object>()));
			var barrier = new Barrier(2);

			var tasks = Enumerable.Range(0, 2).Select(_ => Task.Run<string>(() =>
			{
				IDisposalCollectorArea area = null;

				barrier.SignalAndWait();
				lock (disposalManager) {
					// make sure the two tasks are creating the area one after the other
					area = disposalManager.CreateArea();
				}
				barrier.SignalAndWait();
				var identifier = $"{area.GetType().Name}|{area.GetHashCode()}";
				area.Dispose();
				return identifier;
			})).ToList();

			var results = (await Task.WhenAll(tasks)).ToList();

			results.Count.Should().Be(2);
			results.All(r => r.StartsWith("OrderedAggregateDisposalArea")).Should().BeTrue();
			results[0].Should().NotBe(results[1]);
		}

		[Fact]
		public async Task ServiceDisposal_FromDifferentConcurrentThreads_ShouldDisposeServicesSeparately()
		{
			var taskCount = 7;
			var serviceCount = 100;
			// just create a long list of disposable services and add them to the disposal manager
			var references = Enumerable.Range(0, serviceCount).Select(_ => new NotifiesWhenDisposed()).ToList();
			var activationCache = new FakeActivationCacheAccessor(references);
			var disposalManager = new DisposalManager(activationCache);
			var barrier = new Barrier(taskCount);

			var tasks = Enumerable.Range(0, taskCount).Select(index => Task.Run(() =>
			{
				barrier.SignalAndWait();
				using var area = disposalManager.CreateArea();
				barrier.SignalAndWait();

				foreach (var r in references.Where((_, referenceIndex) => referenceIndex % (taskCount + 1) == index))
				{
					disposalManager.RemoveInstance(new Activation.InstanceReference { Instance = r });
				}
			})).ToList();

			await Task.WhenAll(tasks.ToArray());

			references.Where((_, index) => index % (taskCount + 1) < taskCount).All(r => r.IsDisposed).Should().BeTrue();
			references.Where((_, index) => index % (taskCount + 1) == taskCount).All(r => r.IsDisposed).Should().BeFalse();
		}
	}
}
