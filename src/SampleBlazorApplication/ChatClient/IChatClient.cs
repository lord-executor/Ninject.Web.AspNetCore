namespace SampleBlazorApplication.ChatClient
{
	/// <summary>
	/// See https://docs.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-6.0#strongly-typed-hubs
	/// </summary>
	public interface IChatClient
	{
		Task ReceiveMessage(string user, string message);
	}
}
