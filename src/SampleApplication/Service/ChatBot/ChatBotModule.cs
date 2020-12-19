using Ninject.Modules;

namespace SampleApplication.Service.ChatBot
{
	public class ChatBotModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IChatBotService>().To<ChatBotService>();
			Kernel.Bind<PublishInstruction>().ToConstant(new PublishInstruction(typeof(IChatBotService), "/api/ChatBot"));
		}
	}
}
