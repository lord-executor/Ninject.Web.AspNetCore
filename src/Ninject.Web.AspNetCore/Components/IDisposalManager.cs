using Ninject.Activation;
using Ninject.Components;
using System;

namespace Ninject.Web.AspNetCore.Components
{
	public interface IDisposalManager : INinjectComponent
	{
		void RemoveInstance(InstanceReference instanceReference);

		IDisposalCollectorArea CreateArea();
	}

	public interface IDisposalCollector
	{
		void Register(IActivationEntry activationEntry);
	}

	public interface IDisposalCollectorArea : IDisposalCollector, IDisposable { }
}
