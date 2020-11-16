namespace Ninject.Web.AspNetCore.Test.Fakes
{
	public class Ninja : IWarrior
	{
		public string Name { get; }
		public Ninja(string name)
		{
			Name = name;
		}

	}
}
