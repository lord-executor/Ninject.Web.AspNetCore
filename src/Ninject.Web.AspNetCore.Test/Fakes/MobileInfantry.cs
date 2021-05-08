namespace Ninject.Web.AspNetCore.Test.Fakes
{
	public class MobileInfantry : IWarrior
	{
		public IWeapon Weapon { get; }

		public MobileInfantry(IWeapon weapon)
		{
			Weapon = weapon;
		}

		public string Name => "Rico's Roughnecks";
	}
}
