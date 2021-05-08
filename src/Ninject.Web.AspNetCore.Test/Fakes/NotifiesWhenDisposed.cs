using Ninject.Infrastructure.Disposal;

namespace Ninject.Web.AspNetCore.Test.Fakes
{
	public class NotifiesWhenDisposed : DisposableObject, INotifyWhenDisposed
	{
	}
}
