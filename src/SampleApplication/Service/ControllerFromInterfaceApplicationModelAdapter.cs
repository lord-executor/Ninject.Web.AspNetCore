using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Reflection;

namespace SampleApplication.Service
{
	public class ControllerFromInterfaceApplicationModelAdapter : IApplicationModelAdapter
	{
		private readonly IModelMetadataProvider _modelMetadataProvider;
		private readonly PublishInstruction _publishInstruction;

		public ControllerFromInterfaceApplicationModelAdapter(IModelMetadataProvider modelMetadataProvider, PublishInstruction publishInstruction)
		{
			_modelMetadataProvider = modelMetadataProvider;
			_publishInstruction = publishInstruction;
		}

		public void Apply(ApplicationModel application)
		{
			var controllerModel = CreateControllerModel();
			controllerModel.Application = application;
			application.Controllers.Add(controllerModel);
		}

		private ControllerModel CreateControllerModel()
		{
			var controllerModel = new ControllerModel(_publishInstruction.InterfaceType.GetTypeInfo(), new List<object>());
			var inferMappingConvention = new InferParameterBindingInfoConvention(_modelMetadataProvider);

			foreach (var methodInfo in _publishInstruction.InterfaceType.GetMethods())
			{
				var actionModel = CreateActionModel(_publishInstruction, methodInfo);
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

			return controllerModel;
		}

		private ActionModel CreateActionModel(PublishInstruction publication, MethodInfo methodInfo)
		{
			var result = new ActionModel(methodInfo, new List<object>());
			result.ActionName = methodInfo.Name;
			var selector = new SelectorModel();
			selector.AttributeRouteModel = new AttributeRouteModel
			{
				Name = "R1" + publication.InterfaceType.Name,
				Template = publication.Path,
			};
			result.Selectors.Add(selector);
			return result;
		}

		private ParameterModel CreateParameterModel(ParameterInfo parameterInfo)
		{
			var attributes = parameterInfo.GetCustomAttributes(inherit: true);

			BindingInfo bindingInfo;
			if (_modelMetadataProvider is ModelMetadataProvider modelMetadataProviderBase)
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
