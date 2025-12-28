using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore.Test.Fakes
{
#if NET8_0_OR_GREATER

	public class NinjaWithKeyedWaepon : IWarrior
	{
		public IWeapon Weapon { get; private set; }

		public string Name => nameof(NinjaWithKeyedWaepon) + $" with weapon {Weapon.Type}";

		public NinjaWithKeyedWaepon([FromKeyedServices("Longsword")] IWeapon weapon)
		{
			Weapon = weapon;
		}
	}

#endif
}