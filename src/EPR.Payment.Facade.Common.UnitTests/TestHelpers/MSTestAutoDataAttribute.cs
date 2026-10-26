// MSTest data source that builds test arguments with AutoFixture. AutoFixture publishes xUnit and NUnit
// integrations only, so this adapts AutoFixture's official parameter attributes ([Frozen], [Greedy], ...) to MSTest.
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Payment.Facade.Common.UnitTests.TestHelpers
{
    /// <summary>Supplies auto-generated arguments to an MSTest test method.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MSTestAutoDataAttribute : Attribute, ITestDataSource
    {
        private readonly Func<IFixture> fixtureFactory;

        public MSTestAutoDataAttribute()
            : this(() => new Fixture())
        {
        }

        protected MSTestAutoDataAttribute(Func<IFixture> fixtureFactory)
        {
            this.fixtureFactory = fixtureFactory ?? throw new ArgumentNullException(nameof(fixtureFactory));
        }

        public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(methodInfo);

            var fixture = this.fixtureFactory();
            var arguments = methodInfo.GetParameters().Select(parameter => Resolve(fixture, parameter)).ToArray();
            return new[] { arguments };
        }

        public string? GetDisplayName(MethodInfo methodInfo, object?[]? data) =>
            data is null ? null : $"{methodInfo.Name} ({string.Join(",", data)})";

        private static object? Resolve(IFixture fixture, ParameterInfo parameter)
        {
            // Apply [Frozen] last, so other customisations (e.g. [Greedy]) shape the frozen instance.
            var customizations = parameter.GetCustomAttributes()
                .OfType<IParameterCustomizationSource>()
                .OrderBy(source => source is FrozenAttribute ? 1 : 0);

            foreach (var source in customizations)
            {
                fixture.Customize(source.GetCustomization(parameter));
            }

            return new SpecimenContext(fixture).Resolve(parameter);
        }
    }
}
