using System;
using System.Threading;

namespace Ninject.Web.AspNetCore
{
	public class RequestScope : IDisposable
	{
		private static AsyncLocal<RequestScope> _current = new AsyncLocal<RequestScope>();
		public static RequestScope Current => _current.Value;

		public RequestScope()
		{
			if (_current.Value == null)
			{
				_current.Value = this;
			}
			else
			{
				throw new ApplicationException("Nesting of RequestScope is not allowed.");
			}
		}

		public void Dispose()
		{
			_current.Value = null;
		}
	}
}
