using Ninject.Activation;
using Ninject.Components;
using System;

namespace Ninject.Web.AspNetCore.Components
{
	public interface IDisposalManager : INinjectComponent
	{
		void AddInstance(InstanceReference instanceReference);

		void RemoveInstance(InstanceReference instanceReference);

		IDisposalCollectorArea CreateArea();
	}

	public interface IDisposalCollector
	{
		void Register(InstanceReference instanceReference);
	}

	public interface IDisposalCollectorArea : IDisposalCollector, IDisposable { }
}
