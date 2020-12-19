using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace SampleApplication.Service
{
	public class PublishInstructionApplicationModelConvention : IApplicationModelConvention
	{
		private readonly Lazy<IModelMetadataProvider> _modelMetadataProvider;
		private readonly IList<PublishInstruction> _apiPublications;

		public PublishInstructionApplicationModelConvention(
			// this _must_ be injected as Lazy because it does not exist yet when the type is instantiated
			Lazy<IModelMetadataProvider> modelMetadataProvider,
			IList<PublishInstruction> apiPublications
		) {
			_modelMetadataProvider = modelMetadataProvider;
			_apiPublications = apiPublications;
		}

		public void Apply(ApplicationModel application)
		{
			foreach (var publication in _apiPublications)
			{
				publication.CreateAdapter(_modelMetadataProvider.Value).Apply(application);
			}
		}
	}
}
