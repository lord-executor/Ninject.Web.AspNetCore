using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore.Test.Fakes
{
#if NET8_0_OR_GREATER

	public class NinjaWithKeyedWeapon : IWarrior
	{
		public IKeyedWeaponStorage Storage { get; private set; }
		public IWeapon Weapon { get; private set; }

		public string Name => nameof(NinjaWithKeyedWeapon) + $" with weapon {Weapon.Type}";

		public NinjaWithKeyedWeapon([FromKeyedServices("Longsword")] IWeapon weapon, [FromKeyedServices("Storage")] IKeyedWeaponStorage storage)
		{
			Weapon = weapon;
			Storage = storage;
		}
	}

#endif
}
