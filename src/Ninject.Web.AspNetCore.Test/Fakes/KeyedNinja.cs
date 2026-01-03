using Microsoft.Extensions.DependencyInjection;

namespace Ninject.Web.AspNetCore.Test.Fakes
{
#if NET8_0_OR_GREATER
	public class KeyedNinja : IWarrior
	{
		public object Key {get; private set;}

		public KeyedNinja([ServiceKey] object key)
		{
			Key = key;
		}

		public string Name => nameof(KeyedNinja);
	}
#endif
}