using FluentAssertions;
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
		public void CreateArea_FromDifferentConcurrentThreads_ShouldCreateSeparateRootAreas()
		{
			var disposalManager = new DisposalManager();
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

			var results = tasks.Select(t => t.Result).ToList();

			results.Count.Should().Be(2);
			results.All(r => r.StartsWith("OrderedAggregateDisposalArea")).Should().BeTrue();
			results[0].Should().NotBe(results[1]);
		}

		[Fact]
		public void ServiceDisposal_FromDifferentConcurrentThreads_ShouldDisposeServicesSeparately()
		{
			var taskCount = 7;
			var serviceCount = 100;
			var disposalManager = new DisposalManager();
			var barrier = new Barrier(taskCount);

			// just create a long list of disposable services and add them to the disposal manager
			var references = Enumerable.Range(0, serviceCount).Select(_ => new Activation.InstanceReference { Instance = new NotifiesWhenDisposed() }).ToList();
			foreach (var r in references)
			{
				disposalManager.AddInstance(r);
			}

			var tasks = Enumerable.Range(0, taskCount).Select(index => Task.Run(() =>
			{
				IDisposalCollectorArea area = null;

				barrier.SignalAndWait();
				area = disposalManager.CreateArea();
				barrier.SignalAndWait();

				foreach (var r in references.Where((_, referenceIndex) => referenceIndex % (taskCount + 1) == index))
				{
					area.Register(r);
				}

				area.Dispose();
			})).ToList();

			Task.WaitAll(tasks.ToArray());

			references.Where((_, index) => index % (taskCount + 1) < taskCount).All(r => ((NotifiesWhenDisposed)r.Instance).IsDisposed).Should().BeTrue();
			references.Where((_, index) => index % (taskCount + 1) == taskCount).All(r => ((NotifiesWhenDisposed)r.Instance).IsDisposed).Should().BeFalse();
		}
	}
}
