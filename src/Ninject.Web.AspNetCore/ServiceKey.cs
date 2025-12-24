using System;

namespace Ninject.Web.AspNetCore
{

#if NET8_0_OR_GREATER

	/// <summary>
	/// Used to store ServiceDescriptor.ServiceKey as metadata of the Ninject binding.
	/// </summary>
	public class ServiceKey
	{
		public object Key { get; }

		public ServiceKey(object key)
		{
			Key = key;
		}
	}
#endif

}