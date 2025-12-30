using System;
using System.Linq;
using Ninject.Activation;
using Ninject.Planning.Targets;

namespace Ninject.Web.AspNetCore.RequestActivation;

public static class KeyedRequestExtensions
{
	public static IRequest CreateKeyedChildRequest(this IRequest parentRequest, Type service, object serviceKey,
		IContext parentContext, ITarget target)
	{
		return new KeyedRequest(parentContext, service, serviceKey, target, parentRequest.GetScope);
	}

	public static IRequest ToKeyedRequest(this IRequest request, object serviceKey)
	{
		return new KeyedRequest(request.Service, serviceKey, request.Constraint, request.Parameters.ToList(), 
			request.GetScope, request.IsOptional, request.IsUnique);
	}
}