using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Planning.Bindings;
using Ninject.Planning.Targets;
using Ninject.Web.AspNetCore.Parameters;
using Ninject.Web.AspNetCore.RequestActivation;

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
				return ResolveServiceKeyValue(parent);
			}

			var keyedattributes = GetCustomAttributes(typeof (FromKeyedServicesAttribute), true) as FromKeyedServicesAttribute[];
			if (keyedattributes?.Length > 0)
			{
				return ResolveFromKeyedService(parent, keyedattributes[0]);
			}
#endif
			return base.ResolveWithin(parent);
		}

#if NET8_0_OR_GREATER
		private object ResolveFromKeyedService(IContext parent, FromKeyedServicesAttribute keyedattribute)
		{
			var fromKeyedServiceValue = DeterimeFromKeyedServiceValue(keyedattribute, parent.Parameters);
			var additionalConstraint = fromKeyedServiceValue != null
				? metadata => metadata.DoesMetadataMatchServiceKey(fromKeyedServiceValue)
				: (Func<IBindingMetadata, bool>)null;
			var child = parent.Request.CreateKeyedChildRequest(Type, fromKeyedServiceValue, parent, this,
				additionalConstraint);
			child.IsUnique = true;
			child.IsOptional = false; // constructor arguments marked with FromKeyedServices must always resolve, otherwise an InvalidOperationException is expected.
			try
			{
				return parent.Kernel.Resolve(child).SingleOrDefault();
			}
			catch (ActivationException ex)
			{
				if (Site.HasDefaultValue)
				{
					// in case we have a default value for the constructor parameter, we don't throw but use the default instead.
					return Site.DefaultValue;
				}
				throw new InvalidOperationException(
					$"Can't resolve keyed service of Type {Type} with key {fromKeyedServiceValue}", ex);
			}
		}

		private object DeterimeFromKeyedServiceValue(
			FromKeyedServicesAttribute keyedattribute, ICollection<IParameter> parameters)
		{
#if NET10_0_OR_GREATER
			if (keyedattribute.LookupMode == ServiceKeyLookupMode.NullKey)
			{
				// means no constraint, resolve normally.
				return null;
			}
			if (keyedattribute.LookupMode == ServiceKeyLookupMode.InheritKey)
			{
				var serviceKeyParam = parameters.LastOrDefault(x => x is ServiceKeyParameter) as ServiceKeyParameter;
				return serviceKeyParam?.ServiceKey;
			}
#endif
			return keyedattribute.Key;
		}

		private object ResolveServiceKeyValue(IContext parent)
		{
			var result = parent.Binding.Metadata.GetServiceKey();
			var serviceKeyParameter = parent.Parameters.LastOrDefault(x => x is ServiceKeyParameter) as ServiceKeyParameter;
			if (serviceKeyParameter != null)
			{
				result = serviceKeyParameter.ServiceKey;
			}

			var asConvertible = result as IConvertible;
			if (asConvertible != null)
			{
				try
				{
					result = Convert.ChangeType(asConvertible, this.Type);
				}
				catch (InvalidCastException)
				{
					// we have to throw and InvalidOperationException in this case, a InvalidCastException
					// is not passing the tests
					throw new InvalidOperationException("Cannot convert " + asConvertible + " to " + this.Type);
				}
			}

			if (result != null && !this.Type.IsAssignableFrom(result.GetType()))
			{
				throw new InvalidOperationException("Cannot convert " + result + " to " + this.Type);
			}

			return result;
		}
#endif
	}
}