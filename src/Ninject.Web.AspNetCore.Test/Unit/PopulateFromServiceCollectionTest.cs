using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ninject.Web.AspNetCore.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ninject.Web.AspNetCore.Test.Unit
{
	public class PopulateFromServiceCollectionTest
	{
		
		[Fact]
		public void InstancesAreConvertedToConstants()
		{
			var collection = new ServiceCollection();
			var theSamurai = new Samurai();
			collection.Add(new ServiceDescriptor(typeof(IWarrior), theSamurai));

			var kernel = new StandardKernel();
			kernel.Populate(collection);

			kernel.Get<IWarrior>().Should().BeSameAs(theSamurai);
		}
	}
}
