using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore.Test.Fakes
{
#if NET8_0_OR_GREATER	
	public class KeyedWeaponStorage : IKeyedWeaponStorage
	{
		public IWeapon Weapon { get; private set; }
		public KeyedWeaponStorage([FromKeyedServices("Lance")] IWeapon lance)
		{
			Weapon = lance;
		}
	}
#endif	
}