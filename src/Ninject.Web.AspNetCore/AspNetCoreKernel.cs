using Microsoft.Extensions.DependencyInjection;
using Ninject.Activation;
using Ninject.Activation.Caching;
using Ninject.Activation.Strategies;
using Ninject.Modules;
using Ninject.Planning.Bindings;
using Ninject.Planning.Bindings.Resolvers;
using Ninject.Web.AspNetCore.Components;
using System;
using System.Linq;
using Ninject.Planning.Strategies;
using Ninject.Web.AspNetCore.Parameters;
using Ninject.Web.AspNetCore.Planning;

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
				if (request.IsUnique)
				{
					// as we can't register constraints via microsoft.extensions.dependencyinjection, 
					// we always check for the latest binding
					// Note that we have at least one constraint for the servicekey >= .NET 8.0
					object requestIndexKey = null;
#if NET8_0_OR_GREATER
					var serviceKeyParameter = request.Parameters.LastOrDefault(x => x is ServiceKeyParameter) 
						as ServiceKeyParameter;
					if (serviceKeyParameter != null)
					{
						requestIndexKey = serviceKeyParameter.ServiceKey;
					}
#endif
					latest = binding.Metadata.Get<BindingIndex.Item>(nameof(BindingIndex))?.IsLatest ?? true;
				}
				return binding.Matches(request) && request.Matches(binding) && latest;
			};
		}

		protected override void AddComponents()
		{
			base.AddComponents();
			Components.RemoveAll<IActivationCache>();
			Components.Add<IActivationCache, WeakTableActivationCache>();
			Components.Remove<IBindingResolver, OpenGenericBindingResolver>();
			Components.Add<IBindingResolver, ConstrainedGenericBindingResolver>();
			Components.Remove<IBindingPrecedenceComparer, BindingPrecedenceComparer>();
			Components.Add<IBindingPrecedenceComparer, IndexedBindingPrecedenceComparer>();
			Components.Remove<IPlanningStrategy, ConstructorReflectionStrategy>();
			Components.Add<IPlanningStrategy, ConstructorReflectionStrategyWithKeyedSupport>();

			Components.Add<IDisposalManager, DisposalManager>();
			Components.Remove<IActivationStrategy, DisposableStrategy>();
			Components.Add<IActivationStrategy, OrderedDisposalStrategy>();

#if NET8_0_OR_GREATER
			Components.Add<IMissingBindingResolver, KeyedServiceAnyKeyResolver>();
#endif
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
