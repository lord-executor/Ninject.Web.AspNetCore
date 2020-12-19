using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace SampleApplication.Service
{
	public class PublishInstruction
	{
		/// <summary>
		/// The interface for this API - this interface will be resolved using the DI container
		/// </summary>
		public Type InterfaceType { get; }

		/// <summary>
		/// The path of the API (i.e. without host, port, protocol)
		/// </summary>
		public string Path { get; }

		public PublishInstruction(Type interfaceType, string path)
		{
			InterfaceType = interfaceType;
			Path = $"{path}/{{action}}";
		}

		public IApplicationModelAdapter CreateAdapter(IModelMetadataProvider modelMetadataProvider)
		{
			return new ControllerFromInterfaceApplicationModelAdapter(modelMetadataProvider, this);
		}
	}
}
