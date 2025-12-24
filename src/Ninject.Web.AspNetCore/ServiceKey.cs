using System;

namespace Ninject.Web.AspNetCore
{

#if NET8_0_OR_GREATER

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