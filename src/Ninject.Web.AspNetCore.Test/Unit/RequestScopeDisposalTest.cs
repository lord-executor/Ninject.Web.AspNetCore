using AwesomeAssertions;
using Ninject.Parameters;
using Ninject.Web.AspNetCore.Test.Fakes;
using System.Linq;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class RequestScopeDisposalTest

	{
		[Fact]
		public void UninitializedRequestScopeShouldBeNull()
		{
			RequestScope.Current.Should().BeNull();
		}

		[Fact]
		public void RequestScopeBindingWithoutRequestScopeBecomesTransient()
		{
			var kernel = CreateTestKernel();
			var rico = (MobileInfantry)kernel.Get<IWarrior>();
			var rifle = (MoritaAssaultRifle)kernel.Get<IWeapon>();

			// second instantiated rifle is not the same since there is no request scope
			rico.Weapon.Should().NotBe(rifle);
			rifle.IsDisposed.Should().BeFalse();
		}

		[Fact]
		public void RequestScopeBindingWithRequestCreatesOneInstancePerRequestScope()
		{
			var kernel = CreateTestKernel();

			using (new RequestScope())
			{
				var rico = (MobileInfantry)kernel.Get<IWarrior>();
				var rifle = (MoritaAssaultRifle)kernel.Get<IWeapon>();

				// should get same weapon instance in request scope
				rico.Weapon.Should().Be(rifle);
			}
		}

		[Fact]
		public void DisposableServiceBoundInRequestScopeIsDisposedWhenRequestScopeIs()
		{
			var kernel = CreateTestKernel();
			MoritaAssaultRifle rifle;

			using (new RequestScope())
			{
				var rico = (MobileInfantry)kernel.Get<IWarrior>();
				rifle = (MoritaAssaultRifle)rico.Weapon;
			}

			// since the request scope implements INotifyWhenDisposed, all services that
			// are directly bound to that scope are disposed when the scope is disposed
			rifle.IsDisposed.Should().BeTrue();
		}

		[Fact]
		public void DisposalDoesNotCascadeThroughScopesThatAreNotINotifyWhenDisposed()
		{
			MobileInfantry rico;
			MoritaAssaultRifle rifle;
			var scope = new object();
			var kernel = new StandardKernel(new NinjectSettings() { LoadExtensions = false });
			kernel.Bind<IWarrior>().To<MobileInfantry>().InScope(ctx => RequestScope.Current).WithParameter(new Parameter("scope", scope, true));
			kernel.Bind<IWeapon>().To<MoritaAssaultRifle>().InScope(ctx => ((Parameter)ctx.Parameters.First()).ValueCallback(ctx, null));
			
			using (new RequestScope())
			{
				rico = (MobileInfantry)kernel.Get<IWarrior>();
				rifle = (MoritaAssaultRifle)rico.Weapon;
			}

			// while services that are directly bound to the request scope are disposed
			// when the request scope is disposed, this is not the case for indirect connections
			rifle.IsDisposed.Should().BeFalse();
		}

		[Fact]
		public void DisposalCascadesThroughScopesThatNotifyWhenDisposed()
		{
			MobileInfantry rico;
			MoritaAssaultRifle rifle;
			var scope = new NotifiesWhenDisposed();
			var kernel = new StandardKernel(new NinjectSettings() { LoadExtensions = false });
			kernel.Bind<IWarrior>().To<MobileInfantry>().InScope(ctx => RequestScope.Current).WithParameter(new Parameter("scope", scope, true)).OnDeactivation(_ => scope.Dispose());
			kernel.Bind<IWeapon>().To<MoritaAssaultRifle>().InScope(ctx => ((Parameter)ctx.Parameters.First()).ValueCallback(ctx, null));

			using (new RequestScope())
			{
				rico = (MobileInfantry)kernel.Get<IWarrior>();
				rifle = (MoritaAssaultRifle)rico.Weapon;
			}

			// if scopes are chained to the request scope "properly", then all services
			// are disposed when the request scope is disposed
			rifle.IsDisposed.Should().BeTrue();
		}

		private StandardKernel CreateTestKernel()
		{
			var kernel = new StandardKernel(new NinjectSettings() { LoadExtensions = false });
			kernel.Bind<IWarrior>().To<MobileInfantry>().InScope(ctx => RequestScope.Current);
			kernel.Bind<IWeapon>().To<MoritaAssaultRifle>().InScope(ctx => RequestScope.Current);
			return kernel;
		}
	}
}
