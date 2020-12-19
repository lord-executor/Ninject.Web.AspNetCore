namespace Ninject.Web.AspNetCore.Test.Fakes
{
	public class Knight : IWarrior
	{
		public string Name => $"Knight with {Weapon.Type}";
		public IWeapon Weapon { get; }

		public Knight(IWeapon weapon)
		{
			Weapon = weapon;
		}
	}
}
