using System;

namespace Ninject.Web.AspNetCore
{

	/// <summary>
	/// Used to store ServiceDescriptor.ServiceKey as metadata of the Ninject binding.
	/// Only supported with .NET >= 8.0
	/// </summary>
	public class ServiceKey
	{
		public object Key { get; }

		public ServiceKey(object key)
		{
			Key = key;
		}
	}

}