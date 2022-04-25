using FluentAssertions;
using Ninject.Web.AspNetCore.Test.Fakes;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Regression
{
	public class NinjectBugsRegressionTest
	{
		/// <summary>
		/// See https://github.com/ninject/Ninject/issues/378
		/// Ninject 3.3.4 had a bug where two of the (non-generic) TryGet overloads work differently from
		/// the TryGet&lt;T&gt; versions. Instead of resolving to null if there are multiple matching bindings,
		/// it would run into a LINQ invalid operation exception when calling SingleOrDefault internally.
		/// 
		/// With Ninject 3.5.5 this bug has been fixed and the test here now verifies the correct behavior.
		/// The code of <see cref="NinjectServiceProvider"/> has also been refactored since the discorvery
		/// of this bug in such a way that the code avoids it altogether.
		[Fact]
		public void KernelTryGetWithConstraint_WithMultipleMatchingBindings_ThrowsException()
		{
			var kernel = CreateKernel();
			kernel.Bind<IWarrior>().To<Samurai>();
			kernel.Bind<IWarrior>().To<Ninja>().WithConstructorArgument("name", "Shadow");

			var service = kernel.TryGet(typeof(IWarrior), _ => true);

			service.Should().BeNull();
		}

		private IKernel CreateKernel()
		{
			var settings = new NinjectSettings();
			var kernel = new AspNetCoreKernel(settings);

			return kernel;
		}

	}
}
