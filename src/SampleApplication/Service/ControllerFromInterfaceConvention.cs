using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SampleApplication.Service
{
	public class ControllerFromInterfaceConvention : IApplicationModelConvention
	{
		private readonly Lazy<IModelMetadataProvider> _modelMetadataProvider;
		private readonly IList<PublishInstruction> _apiPublications;

		public ControllerFromInterfaceConvention(
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
				AddControllerModel(application, publication);
			}
		}

		private void AddControllerModel(ApplicationModel application, PublishInstruction publication)
		{
			var controllerModel = new ControllerModel(publication.InterfaceType.GetTypeInfo(), new List<object>());
			controllerModel.Application = application;

			var inferMappingConvention = new InferParameterBindingInfoConvention(_modelMetadataProvider.Value);

			foreach (var methodInfo in publication.InterfaceType.GetMethods())
			{
				var actionModel = CreateActionModel(publication, methodInfo);
				if (actionModel == null)
				{
					continue;
				}

				actionModel.Controller = controllerModel;
				controllerModel.Actions.Add(actionModel);

				foreach (var parameterInfo in actionModel.ActionMethod.GetParameters())
				{
					var parameterModel = CreateParameterModel(parameterInfo);
					if (parameterModel != null)
					{
						parameterModel.Action = actionModel;
						actionModel.Parameters.Add(parameterModel);
					}
				}

				inferMappingConvention.Apply(actionModel);
			}


			application.Controllers.Add(controllerModel);
		}

		private ActionModel CreateActionModel(PublishInstruction publication, MethodInfo methodInfo)
		{
			var result = new ActionModel(methodInfo, new List<object>());
			result.ActionName = methodInfo.Name;
			var selector = new SelectorModel();
			selector.AttributeRouteModel = new AttributeRouteModel
			{
				Name = "R1" + publication.InterfaceType.Name,
				Template = publication.Path + "/{action}"
			};
			result.Selectors.Add(selector);
			return result;
		}

		private ParameterModel CreateParameterModel(ParameterInfo parameterInfo)
		{
			var attributes = parameterInfo.GetCustomAttributes(inherit: true);

			BindingInfo bindingInfo;
			if (_modelMetadataProvider.Value is ModelMetadataProvider modelMetadataProviderBase)
			{
				var modelMetadata = modelMetadataProviderBase.GetMetadataForParameter(parameterInfo);
				bindingInfo = BindingInfo.GetBindingInfo(attributes, modelMetadata);
			}
			else
			{
				// GetMetadataForParameter should only be used if the user has opted in to the 2.1 behavior.
				bindingInfo = BindingInfo.GetBindingInfo(attributes);
			}

			var parameterModel = new ParameterModel(parameterInfo, attributes)
			{
				ParameterName = parameterInfo.Name,
				BindingInfo = bindingInfo,
			};

			return parameterModel;
		}
	}
}
