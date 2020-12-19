using Ninject.Activation;
using Ninject.Modules;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using System;
using System.Collections.Generic;

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
	}
}
