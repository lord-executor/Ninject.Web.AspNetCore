using System;
using System.Reflection;
using Ninject.Components;
using Ninject.Infrastructure.Language;
using Ninject.Injection;
using Ninject.Planning;
using Ninject.Planning.Directives;
using Ninject.Planning.Strategies;
using Ninject.Selection;
using Ninject.Web.AspNetCore.Planning;

namespace Ninject.Web.AspNetCore.Components
{
	/// <summary>
	/// Adds a directive to plans indicating which constructor should be injected during activation.
	/// Need a custom one to support FromKeyedServices attribute, which doesn't inherit from ConstraintAttribute
	/// </summary>
	public class ConstructorReflectionStrategyWithKeyedSupport : NinjectComponent,
		IPlanningStrategy
	{
		/// <summary>
        /// Initializes a new instance of the <see cref="ConstructorReflectionStrategy"/> class.
        /// </summary>
        /// <param name="selector">The selector component.</param>
        /// <param name="injectorFactory">The injector factory component.</param>
        public ConstructorReflectionStrategyWithKeyedSupport(ISelector selector, IInjectorFactory injectorFactory)
        {
            Selector = selector;
            InjectorFactory = injectorFactory;
        }

        /// <summary>
        /// Gets the selector component.
        /// </summary>
        public ISelector Selector { get; }

        /// <summary>
        /// Gets or sets the injector factory component.
        /// </summary>
        public IInjectorFactory InjectorFactory { get; }

        /// <summary>
        /// Adds a <see cref="ConstructorInjectionDirective"/> to the plan for the constructor
        /// that should be injected.
        /// </summary>
        /// <param name="plan">The plan that is being generated.</param>
        public void Execute(IPlan plan)
        {
            var constructors = Selector.SelectConstructorsForInjection(plan.Type);
            if (constructors == null)
            {
                return;
            }

            foreach (ConstructorInfo constructor in constructors)
            {
                var hasInjectAttribute = constructor.HasAttribute(Settings!.InjectAttribute);
                var hasObsoleteAttribute = constructor.HasAttribute<ObsoleteAttribute>();
                var directive = new ConstructorInjectionDirectiveWithKeyedSupport(constructor, InjectorFactory.Create(constructor))
                {
                    HasInjectAttribute = hasInjectAttribute,
                    HasObsoleteAttribute = hasObsoleteAttribute,
                };

                plan.Add(directive);
            }
        }
	}
}