using FluentAssertions;
using Moq;
using Ninject.Parameters;
using Ninject.Syntax;
using Ninject.Web.AspNetCore.Test.Fakes;
using System.Linq;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class ServiceProviderResolutionRootTest
	{
		[Fact]
		public void ScopeResolutionRoot_Resolution_IsDelegatedToParent()
		{
			var scopeRoot = CreateScopeResolutionRoot();

			var request = scopeRoot.CreateRequest(typeof(IWarrior), null, Enumerable.Empty<IParameter>(), false, true);

			scopeRoot.CanResolve(request).Should().BeTrue();
			var warriors = scopeRoot.Resolve(request);
			warriors.Should().HaveCount(1);
			warriors.Single().Should().BeOfType<Samurai>();
		}

		[Fact]
		public void ScopeResolutionRoot_RequestWithoutParameters_ShouldAddScopeParameter()
		{
			var scopeRoot = CreateScopeResolutionRoot();

			var request = scopeRoot.CreateRequest(typeof(IWarrior), null, Enumerable.Empty<IParameter>(), false, true);

			request.Parameters.Should().HaveCount(1).And.AllBeOfType<ServiceProviderScopeParameter>();
		}

		[Fact]
		public void ScopeResolutionRoot_RequestWithParameters_ShouldAddScopeParameter()
		{
			var scopeRoot = CreateScopeResolutionRoot();

			var request = scopeRoot.CreateRequest(typeof(IWarrior), null, new[] { new Parameter("foo", (object)null, false) }, false, true);

			request.Parameters.Should().HaveCount(2).And
				.Contain(p => p.Name == "foo").And
				.Contain(p => p.Name == nameof(ServiceProviderScopeParameter));
		}

		private ServiceProviderScopeResolutionRoot CreateScopeResolutionRoot()
		{
			var kernel = CreateKernel();
			var scope = new NinjectServiceScope(kernel, true);
			return new ServiceProviderScopeResolutionRoot(kernel, scope);
		}

		private IKernel CreateKernel()
		{
			var kernel = new AspNetCoreKernel(new NinjectSettings { LoadExtensions = false });

			kernel.Bind<IWarrior>().To<Samurai>();

			return kernel;
		}
	}
}
