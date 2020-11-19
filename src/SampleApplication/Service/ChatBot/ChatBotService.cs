using System;
using System.Collections.Generic;

namespace SampleApplication.Service.ChatBot
{
	public class ChatBotService : IChatBotService
	{
		private static readonly IDictionary<HelloType, string> _messageTemplates = new Dictionary<HelloType, string>
		{
			[HelloType.Normal] = "Hello {0}.",
			[HelloType.Casual] = "Hey {0}, what up?",
			[HelloType.Rude] = "Talk to the hand {0}!",
		};

		public HelloResponse SayHello(HelloRequest request)
		{
			return new HelloResponse
			{
				HelloMessage = String.Format(_messageTemplates[request.Type], request.Name),
				From = "ChatBot 1.0"
			};
		}
	}
}
