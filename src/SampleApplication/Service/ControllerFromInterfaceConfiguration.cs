using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SampleApplication.Service
{
	public class ControllerFromInterfaceConfiguration : IConfigureOptions<MvcOptions>
	{
		private readonly ControllerFromInterfaceConvention _convention;

		public ControllerFromInterfaceConfiguration(ControllerFromInterfaceConvention convention)
		{
			_convention = convention;
		}

		public void Configure(MvcOptions options)
		{
			options.Conventions.Add(_convention);
		}
	}
}
