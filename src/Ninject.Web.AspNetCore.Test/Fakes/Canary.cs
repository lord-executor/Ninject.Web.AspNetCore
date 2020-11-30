using System;

namespace Ninject.Web.AspNetCore.Test.Fakes
{
	public class Canary : IWarrior, IDisposable
	{
		public bool IsDisposed { get; private set; }

		public string Name => nameof(Canary);

		public void Dispose()
		{
			IsDisposed = true;
		}
	}
}
