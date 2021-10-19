using Ninject.Modules;
using Ninject.Web.Common;

namespace SampleApplication.Service.ChatBot
{
	public class ChatBotModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IChatBotService>().To<ChatBotService>().InRequestScope();
			Kernel.Bind<PublishInstruction>().ToConstant(new PublishInstruction(typeof(IChatBotService), "/api/ChatBot"));
		}
	}
}
