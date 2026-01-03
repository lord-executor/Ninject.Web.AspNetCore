namespace Ninject.Web.AspNetCore.Test.Fakes
{
#if NET8_0_OR_GREATER	
	public interface IKeyedWeaponStorage
	{
		IWeapon Weapon { get; }
	}
#endif	
}