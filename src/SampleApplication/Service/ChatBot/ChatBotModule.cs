using Ninject.Modules;

namespace SampleApplication.Service.ChatBot
{
	public class ChatBotModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IChatBotService>().To<ChatBotService>();
			Kernel.Bind<ApiPublication>().ToConstant(new ApiPublication(typeof(IChatBotService), "/api/ChatBot"));
		}
	}
}
