using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SampleApplication.Service
{
	public class PublishInstructionApplicationModelConfiguration : IConfigureOptions<MvcOptions>
	{
		private readonly PublishInstructionApplicationModelConvention _convention;

		public PublishInstructionApplicationModelConfiguration(PublishInstructionApplicationModelConvention convention)
		{
			_convention = convention;
		}

		public void Configure(MvcOptions options)
		{
			options.Conventions.Add(_convention);
		}
	}
}
