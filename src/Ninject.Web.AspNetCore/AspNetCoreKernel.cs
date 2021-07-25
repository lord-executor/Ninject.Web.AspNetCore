using Ninject.Activation;
using Ninject.Activation.Caching;
using Ninject.Modules;
using Ninject.Planning.Bindings;
using Ninject.Planning.Bindings.Resolvers;
using System;

namespace Ninject.Web.AspNetCore
{
	public class AspNetCoreKernel : StandardKernel
	{
		public AspNetCoreKernel(params INinjectModule[] modules)
			: base(modules) { }

		public AspNetCoreKernel(INinjectSettings settings, params INinjectModule[] modules)
			: base(settings, modules) { }

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
			Components.Remove<ICache, Cache>();
			Components.Add<ICache, OrderedCache>();
		}
	}
}
