using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Activation;
using Ninject.Planning.Bindings;
using Ninject.Planning.Targets;

namespace Ninject.Web.AspNetCore.Planning
{
	public class ParameterTargetWithKeyedSupport : ParameterTarget, ITarget
	{
		public ParameterTargetWithKeyedSupport(MethodBase method, ParameterInfo site) : base(method, site)
		{
		}

		public override bool HasDefaultValue
		{
			get
			{
				var result = this.Site.HasDefaultValue;
#if NET8_0_OR_GREATER
				// ensure that constructor scorer knows that we have a default value for parameters decorated with ServiceKey
				// as the DefaultValueBindingResolver is only a MissingBindingResolver, the
				// ParameterTargetWithKeyedSupport.ResolveWithin method already
				// provided a default value before any Ninject resolution for the value happens.
				result = result || GetCustomAttributes(typeof (ServiceKeyAttribute), true)?.Length > 0;
#endif
				return result;
			}
		}

		protected override Func<IBindingMetadata, bool> ReadConstraintFromTarget()
		{
#if NET8_0_OR_GREATER
			var keyedattributes = GetCustomAttributes(typeof (FromKeyedServicesAttribute), true) as FromKeyedServicesAttribute[];
			var baseFunc = base.ReadConstraintFromTarget();
			if (keyedattributes == null || keyedattributes.Length == 0)
			{
				return baseFunc;
			}

			return metadata =>
			{
				var result = true;
				if (baseFunc != null)
				{
					result = baseFunc(metadata);
				}

				if (metadata.HasServiceKeyMetadata())
				{
					result = result && metadata.DoesMetadataMatchServiceKey(keyedattributes[0].Key, true);
				}

				return result;
			};
#else
			return base.ReadConstraintFromTarget();
#endif
		}

		/// <summary>
		/// MethodInjectionStrategy.GetMethodArguments calls ITarget.ResolveWithin.
		/// As we can't override the base implementation as it is not virtual, the
		/// explicit interface implementation helps to still delegate the resolution to here.
		/// </summary>
		object ITarget.ResolveWithin(IContext parent)
		{
#if NET8_0_OR_GREATER
			var serviceKeyAttributes = GetCustomAttributes(typeof (ServiceKeyAttribute), true) as ServiceKeyAttribute[];
			if (serviceKeyAttributes?.Length > 0)
			{
				var result = parent.Binding.Metadata.GetServiceKey();
				if (result == KeyedService.AnyKey && this.Type == typeof(string))
				{
					// expected to automatically convert from AnyKey to string representation
					result = KeyedService.AnyKey.ToString();
				}

				return result;
			}
#endif
			return base.ResolveWithin(parent);
		}
	}
}