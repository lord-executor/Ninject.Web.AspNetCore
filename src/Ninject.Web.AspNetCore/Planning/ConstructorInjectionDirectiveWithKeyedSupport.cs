using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject.Injection;
using Ninject.Planning.Directives;
using Ninject.Planning.Targets;

namespace Ninject.Web.AspNetCore.Planning
{
	public class ConstructorInjectionDirectiveWithKeyedSupport : ConstructorInjectionDirective
	{
		public ConstructorInjectionDirectiveWithKeyedSupport(ConstructorInfo constructor, ConstructorInjector injector) : base(constructor, injector)
		{
		}

		protected override ITarget[] CreateTargetsFromParameters(ConstructorInfo method)
		{
			return method.GetParameters().
				Select((Func<ParameterInfo, ParameterTarget>) (parameter => new ParameterTargetWithKeyedSupport(method, parameter))).
				ToArray();
		}
	}
}