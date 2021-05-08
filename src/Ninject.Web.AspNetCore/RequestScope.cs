using Ninject.Infrastructure.Disposal;
using System;
using System.Threading;

namespace Ninject.Web.AspNetCore
{
	public class RequestScope : DisposableObject
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

		public override void Dispose(bool disposing)
		{
			if (disposing && !IsDisposed)
			{
				_current.Value = null;
			}

			base.Dispose(disposing);
		}
	}
}
