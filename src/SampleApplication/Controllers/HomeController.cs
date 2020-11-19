using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using SampleApplication_AspNetCore.Models;

namespace SampleApplication_AspNetCore.Controllers
{
	public class HomeController : Controller
	{
		private readonly IServer _server;
		private readonly IServiceProvider _serviceProvider;

		public HomeController(IServer server, IServiceProvider serviceProvider)
		{
			_server = server;
			_serviceProvider = serviceProvider;
		}

		public IActionResult Index()
		{
			ViewData["EntryAssembly"] = Assembly.GetEntryAssembly().FullName;
			ViewData["ProcessName"] = Process.GetCurrentProcess().ProcessName;
			ViewData["Server"] = _server.GetType().FullName;
			ViewData["ServiceProvider"] = _serviceProvider.GetType().FullName;
			ViewData["dotNETVersion"] = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
			ViewData["Environment"] = Environment.GetEnvironmentVariables();
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
