using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ninject.Web.AspNetCore
{
	public class ServiceProviderScopeResolutionRoot : IResolutionRoot
	{
		private readonly IResolutionRoot _parent;
		private readonly ServiceProviderScopeParameter _scopeParameter;

		public ServiceProviderScopeResolutionRoot(IResolutionRoot parent, NinjectServiceScope scope)
		{
			_parent = parent;
			_scopeParameter = new ServiceProviderScopeParameter(scope);
		}

		public bool CanResolve(IRequest request)
		{
			return _parent.CanResolve(request);
		}

		public bool CanResolve(IRequest request, bool ignoreImplicitBindings)
		{
			return _parent.CanResolve(request, ignoreImplicitBindings);
		}

		public IRequest CreateRequest(Type service, Func<IBindingMetadata, bool> constraint, IEnumerable<IParameter> parameters, bool isOptional, bool isUnique)
		{
			var updatedParameters = parameters.ToList() ?? new List<IParameter>();
			updatedParameters.Add(_scopeParameter);
			return _parent.CreateRequest(service, constraint, updatedParameters, isOptional, isUnique);
		}

		public void Inject(object instance, params IParameter[] parameters)
		{
			_parent.Inject(instance, parameters);
		}

		public bool Release(object instance)
		{
			return _parent.Release(instance);
		}

		public IEnumerable<object> Resolve(IRequest request)
		{
			return _parent.Resolve(request);
		}
	}
}
