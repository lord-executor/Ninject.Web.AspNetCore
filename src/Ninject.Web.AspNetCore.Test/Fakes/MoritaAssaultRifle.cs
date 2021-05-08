using System;

namespace Ninject.Web.AspNetCore.Test.Fakes
{
	public class MoritaAssaultRifle : IWeapon, IDisposable
	{
		public bool IsDisposed { get; private set; }

		public string Type => nameof(MoritaAssaultRifle);

		public void Dispose()
		{
			IsDisposed = true;
		}
	}
}
