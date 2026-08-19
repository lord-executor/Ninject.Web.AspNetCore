using Ninject.Parameters;


namespace Ninject.Web.AspNetCore.Parameters
{
	public class ServiceKeyParameter : Parameter
	{
		public ServiceKeyParameter(object value) : base(nameof(ServiceKeyParameter), value, true)
		{
			ServiceKey = value;
		}

		public object ServiceKey { get; }
	}
}
