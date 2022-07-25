using Microsoft.AspNetCore.SignalR;
using SampleBlazorApplication.ChatClient;

namespace SampleBlazorApplication.Data
{
    public class WeatherForecastService
    {
		/// <summary>
		/// The _chatHubContext is only here to test the _injection of strongly typed SignalR hub contexts as described in
		/// https://docs.microsoft.com/en-us/aspnet/core/signalr/hubcontext?view=aspnetcore-6.0#inject-a-strongly-typed-hubcontext
		/// 
		/// This is essentially a regression test for https://github.com/lord-executor/Ninject.Web.AspNetCore/issues/10
		/// </summary>
		private readonly IHubContext<StronglyTypedChatHub, IChatClient> _chatHubContext;

		public WeatherForecastService(IHubContext<StronglyTypedChatHub, IChatClient> chatHubContext)
		{
			_chatHubContext = chatHubContext;
		}

        private static readonly string[] Summaries = new[]
        {
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

        public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
        {
            return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray());
        }
    }
}