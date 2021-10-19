using Microsoft.Extensions.DependencyInjection;
using Ninject.Activation;
using Ninject.Activation.Strategies;
using Ninject.Modules;
using Ninject.Planning.Bindings;
using Ninject.Planning.Bindings.Resolvers;
using Ninject.Web.AspNetCore.Components;
using System;

namespace Ninject.Web.AspNetCore
{
	public class AspNetCoreKernel : StandardKernel, IServiceScopeFactory
	{
		public IServiceScope RootScope { get; }

		public AspNetCoreKernel(params INinjectModule[] modules)
			: base(modules)
		{
			RootScope = new NinjectServiceScope(this, true);
			Settings.AllowNullInjection = true;
		}

		public AspNetCoreKernel(INinjectSettings settings, params INinjectModule[] modules)
			: base(settings, modules)
		{
			RootScope = new NinjectServiceScope(this, true);
			Settings.AllowNullInjection = true;
		}

		protected override Func<IBinding, bool> SatifiesRequest(IRequest request)
		{
			return binding => {
				var latest = true;
				if (request.IsUnique && request.Constraint == null)
				{
					latest = binding.Metadata.Get<BindingIndex.Item>(nameof(BindingIndex))?.IsLatest ?? true;
				}
				return binding.Matches(request) && request.Matches(binding) && latest;
			};
		}

		protected override void AddComponents()
		{
			base.AddComponents();
			Components.Remove<IBindingResolver, OpenGenericBindingResolver>();
			Components.Add<IBindingResolver, ConstrainedGenericBindingResolver>();
			Components.Remove<IBindingPrecedenceComparer, BindingPrecedenceComparer>();
			Components.Add<IBindingPrecedenceComparer, IndexedBindingPrecedenceComparer>();

			Components.Add<IDisposalManager, DisposalManager>();
			Components.Remove<IActivationStrategy, DisposableStrategy>();
			Components.Add<IActivationStrategy, OrderedDisposalStrategy>();
		}

		public void DisableAutomaticSelfBinding()
		{
			Components.Remove<IMissingBindingResolver, SelfBindingResolver>();
		}

		public override void Dispose(bool disposing)
		{
			if (disposing && !IsDisposed)
			{
				RootScope.Dispose();
			}

			base.Dispose(disposing);
		}

		public IServiceScope CreateScope()
		{
			return new NinjectServiceScope(this, false);
		}
	}
}
