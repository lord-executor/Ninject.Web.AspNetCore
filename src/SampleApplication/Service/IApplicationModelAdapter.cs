using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SampleApplication.Service
{
	public interface IApplicationModelAdapter
	{
		void Apply(ApplicationModel application);
	}
}
