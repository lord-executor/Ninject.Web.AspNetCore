namespace SampleApplication.Service.ChatBot
{
	public interface IChatBotService
	{
		HelloResponse SayHello(HelloRequest request);
	}
}
