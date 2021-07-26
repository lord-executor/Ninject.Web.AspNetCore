#if NET6_0
using Microsoft.Extensions.DependencyInjection;
using Ninject.Syntax;
using System;

namespace Ninject.Web.AspNetCore
{

	public partial class NinjectServiceProvider : IServiceProviderIsService
	{
	}
}

#endif