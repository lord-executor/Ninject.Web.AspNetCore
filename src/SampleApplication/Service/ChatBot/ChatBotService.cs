using System;
using System.Collections.Generic;
using System.Globalization;

namespace SampleApplication.Service.ChatBot
{
	public class ChatBotService : IChatBotService
	{
		private static readonly Dictionary<HelloType, string> MessageTemplates = new Dictionary<HelloType, string>
		{
			[HelloType.Normal] = "Hello {0}.",
			[HelloType.Casual] = "Hey {0}, what up?",
			[HelloType.Rude] = "Talk to the hand {0}!",
		};

		public HelloResponse SayHello(HelloRequest request)
		{
			if (request == null || String.IsNullOrEmpty(request.Name))
			{
				throw new ArgumentException("Request must contain a non-empty Name");
			}

			return new HelloResponse
			{
				HelloMessage = string.Format(CultureInfo.InvariantCulture, MessageTemplates[request.Type], request.Name),
				From = "ChatBot 1.0"
			};
		}
	}
}
