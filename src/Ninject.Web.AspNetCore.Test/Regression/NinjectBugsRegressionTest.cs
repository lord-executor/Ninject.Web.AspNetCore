using FluentAssertions;
using Ninject.Web.AspNetCore.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Regression
{
	public class NinjectBugsRegressionTest
	{
		/// <summary>
		/// See https://github.com/ninject/Ninject/issues/378
		/// Ninject 3.3.4 has a bug where two of the (non-generic) TryGet overloads work differently from
		/// the TryGet&lt;T&gt; versions. Instead of resolving to null if there are multiple matching bindings,
		/// it will run into a LINQ invalid operation exception when calling SingleOrDefault internally.
		/// 
		/// If this test FAILS, then this means that the bug was probably fixed and the code in
		/// <see cref="NinjectServiceProvider.GetService(Type)"/> can be simplified by removing the workaround
		/// for this bug.
		/// </summary>
		[Fact]
		public void KernelTryGetWithConstraint_WithMultipleMatchingBindings_ThrowsException()
		{
			var kernel = CreateKernel();
			kernel.Bind<IWarrior>().To<Samurai>();
			kernel.Bind<IWarrior>().To<Ninja>().WithConstructorArgument("name", "Shadow");

			Action action = () => kernel.TryGet(typeof(IWarrior), _ => true);

			action.Should().Throw<InvalidOperationException>();
		}

		private IKernel CreateKernel()
		{
			var settings = new NinjectSettings();
			var kernel = new StandardKernel(settings);

			return kernel;
		}

	}
}
