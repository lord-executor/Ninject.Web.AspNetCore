using System;

namespace Ninject.Web.AspNetCore.Test.Fakes
{
	public class MoritaAssaultRifle : IWeapon, IDisposable
	{
		public bool IsDisposed { get; private set; }

		public void Attack()
		{
			Console.WriteLine("Pew Pew!");
		}

		public void Dispose()
		{
			IsDisposed = true;
		}
	}
}
