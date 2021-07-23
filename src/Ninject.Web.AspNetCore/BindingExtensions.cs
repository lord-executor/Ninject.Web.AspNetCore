using Ninject.Activation;
using Ninject.Syntax;
using System;

namespace Ninject.Web.AspNetCore
{
	public static class BindingExtensions
	{
		public static IBindingInNamedWithOrOnSyntax<T> WhenGenericsMatch<T>(this IBindingWhenSyntax<T> binding, Type boundType)
		{
			if (boundType.ContainsGenericParameters)
			{
				return binding.When((IRequest request) =>
				{
					var genericArguments = boundType.GetGenericArguments();
					var realArguments = request.Service.GenericTypeArguments;
					for (var i = 0; i < genericArguments.Length; i++)
					{
						foreach (var constraint in genericArguments[i].GetGenericParameterConstraints())
						{
							if (!constraint.IsAssignableFrom(realArguments[i]))
							{
								return false;
							}
						}
					}
					return true;
				});
			}

			return (IBindingInNamedWithOrOnSyntax<T>)binding;
		}
	}
}
