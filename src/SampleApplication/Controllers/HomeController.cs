using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Ninject;
using Ninject.Activation.Caching;
using Ninject.Web.AspNetCore.Components;
using SampleApplication.Models;

namespace SampleApplication.Controllers
{
	public class HomeController : Controller
	{
		private readonly IServer _server;
		private readonly IServiceProvider _serviceProvider;
		private readonly IDisposalManager _disposalManager;

		public HomeController(IServer server, IServiceProvider serviceProvider, IKernel kernel)
		{
			_server = server;
			_serviceProvider = serviceProvider;
			_disposalManager = kernel.Components.Get<IDisposalManager>();
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

		public IActionResult Services()
		{
			return View();
		}

		public IActionResult Memory()
		{
			var alive = (_disposalManager as DisposalManager)?.GetActiveReferences().Where(r => r.IsAlive).ToList();
			var dead = (_disposalManager as DisposalManager)?.GetActiveReferences().Where(r => !r.IsAlive).ToList();

			var process = Process.GetCurrentProcess();

			ViewData["Memory"] = new Dictionary<string, string>
			{
				["NonpagedSystemMemorySize64"] = $"{process.NonpagedSystemMemorySize64 / (1024 * 1024)} MB",
				["PagedMemorySize64"] = $"{process.PagedMemorySize64 / (1024 * 1024)} MB",
				["PagedSystemMemorySize64"] = $"{process.PagedSystemMemorySize64 / (1024 * 1024)} MB",
				["PrivateMemorySize64"] = $"{process.PrivateMemorySize64 / (1024 * 1024)} MB",
			};

			ViewData["ServiceReferences"] = new Dictionary<string, string>
			{
				["Alive"] = alive.Count.ToString(),
				["Dead"] = dead.Count.ToString(),
			};

			return View();
		}

		public IActionResult RunGC()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			return RedirectToAction(nameof(Memory));
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
