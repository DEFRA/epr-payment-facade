using System.Reflection;
using AutoFixture;
using AutoFixture.Xunit2;

namespace EPR.Payment.Facade.Common.UnitTests.TestHelpers
{
    [TestClass]
    public class MSTestAutoDataAttributeTests
    {
        // Holds a delegate, so MSTest cannot serialize it. MSTest copies data rows whose arguments are all serializable,
        // which loses [Frozen] identity (the unofficial package this replaced behaved the same way); rows with an Opaque keep it.
        public class Opaque
        {
            public Action? Callback { get; set; }
        }

        public class Widget
        {
            public string Value { get; set; } = string.Empty;
        }

        public class Labelled
        {
            public Labelled(string label)
            {
                Label = label;
            }

            public string Label { get; }
        }

        public class Gadget
        {
            public Gadget()
            {
                Constructor = "modest";
            }

            public Gadget(int number, string text)
            {
                Constructor = "greedy";
            }

            public string Constructor { get; }
        }

        public class CustomFactoryAttribute : MSTestAutoDataAttribute
        {
            public CustomFactoryAttribute()
                : base(() =>
                {
                    var fixture = new Fixture();
                    fixture.Customize<Widget>(c => c.With(w => w.Value, "from-factory"));
                    return fixture;
                })
            {
            }
        }

        public class NullFactoryAttribute : MSTestAutoDataAttribute
        {
            public NullFactoryAttribute()
                : base(null!)
            {
            }
        }

        public class Samples
        {
            public void NoParameters()
            {
            }

            public void PlainParameters(string text, int number)
            {
            }

            public void CustomFactory(Widget widget)
            {
            }

            public void Frozen([Frozen] Widget first, Widget second)
            {
            }

            public void FrozenString([Frozen] string text, Labelled labelled)
            {
            }

            public void FrozenParameterName([Frozen(Matching.ParameterName)] string label, string other, Labelled labelled)
            {
            }

            public void FrozenThenGreedy([Frozen, Greedy] Gadget first, Gadget second)
            {
            }

            public void GreedyThenFrozen([Greedy, Frozen] Gadget first, Gadget second)
            {
            }
        }

        private static MethodInfo Method(string name) => typeof(Samples).GetMethod(name)!;

        private static object?[] FirstRow(MSTestAutoDataAttribute attribute, string method) => attribute.GetData(Method(method)).Single();

        [TestMethod]
        [MSTestAutoData]
        public void AutoData_UsedAsTestAttribute_SuppliesFrozenAndGeneratedArguments(string text, int number, [Frozen] Opaque first, Opaque second)
        {
            Assert.IsFalse(string.IsNullOrEmpty(text));
            Assert.AreNotEqual(0, number);
            Assert.AreSame(first, second);
        }

        [TestMethod]
        public void GetData_ReturnsOneRowWithAnArgumentPerParameter()
        {
            var rows = new MSTestAutoDataAttribute().GetData(Method(nameof(Samples.PlainParameters))).ToList();

            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual(2, rows[0].Length);
            Assert.IsInstanceOfType<string>(rows[0][0]);
            Assert.IsInstanceOfType<int>(rows[0][1]);
        }

        [TestMethod]
        public void GetData_WithNoParameters_ReturnsOneEmptyRow()
        {
            var rows = new MSTestAutoDataAttribute().GetData(Method(nameof(Samples.NoParameters))).ToList();

            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual(0, rows[0].Length);
        }

        [TestMethod]
        public void GetData_WithNullMethod_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MSTestAutoDataAttribute().GetData(null!).ToList());
        }

        [TestMethod]
        public void Constructor_WithNullFactory_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new NullFactoryAttribute());
        }

        [TestMethod]
        public void GetData_UsesTheSuppliedFixtureFactory()
        {
            var row = FirstRow(new CustomFactoryAttribute(), nameof(Samples.CustomFactory));

            Assert.AreEqual("from-factory", ((Widget)row[0]!).Value);
        }

        [TestMethod]
        public void GetData_UsesAFreshFixtureForEachCall()
        {
            var attribute = new MSTestAutoDataAttribute();

            var first = (Widget)FirstRow(attribute, nameof(Samples.Frozen))[0]!;
            var second = (Widget)FirstRow(attribute, nameof(Samples.Frozen))[0]!;

            Assert.AreNotSame(first, second);
        }

        [TestMethod]
        public void GetDisplayName_WithoutData_ReturnsNullSoMsTestUsesTheMethodName()
        {
            var name = new MSTestAutoDataAttribute().GetDisplayName(Method(nameof(Samples.PlainParameters)), null);

            Assert.IsNull(name);
        }

        [TestMethod]
        public void GetDisplayName_WithData_ListsArguments()
        {
            var name = new MSTestAutoDataAttribute().GetDisplayName(Method(nameof(Samples.PlainParameters)), new object?[] { "abc", 7 });

            Assert.AreEqual("PlainParameters (abc,7)", name);
        }

        [TestMethod]
        public void GetData_AppliesFrozenFromAutoFixtureXunit2()
        {
            var row = FirstRow(new MSTestAutoDataAttribute(), nameof(Samples.Frozen));

            Assert.AreSame(row[0], row[1]);
        }

        [TestMethod]
        public void GetData_FrozenStringReachesConstructorParameters()
        {
            var row = FirstRow(new MSTestAutoDataAttribute(), nameof(Samples.FrozenString));

            Assert.AreEqual(row[0], ((Labelled)row[1]!).Label);
        }

        [TestMethod]
        public void GetData_ResolvesEachArgumentThroughItsParameter()
        {
            var row = FirstRow(new MSTestAutoDataAttribute(), nameof(Samples.PlainParameters));

            // AutoFixture seeds a string with the name of the parameter it is resolving.
            StringAssert.StartsWith((string)row[0]!, "text");
        }

        [TestMethod]
        public void GetData_FrozenParameterName_ReachesOnlyTheMatchingConstructorParameter()
        {
            var row = FirstRow(new MSTestAutoDataAttribute(), nameof(Samples.FrozenParameterName));

            Assert.AreEqual(row[0], ((Labelled)row[2]!).Label);
            Assert.AreNotEqual(row[0], row[1]);
        }

        [TestMethod]
        public void GetData_FrozenThenGreedy_FreezesTheGreedilyBuiltInstance()
        {
            var row = FirstRow(new MSTestAutoDataAttribute(), nameof(Samples.FrozenThenGreedy));

            Assert.AreEqual("greedy", ((Gadget)row[0]!).Constructor);
            Assert.AreSame(row[0], row[1]);
        }

        [TestMethod]
        public void GetData_GreedyThenFrozen_FreezesTheGreedilyBuiltInstance()
        {
            var row = FirstRow(new MSTestAutoDataAttribute(), nameof(Samples.GreedyThenFrozen));

            Assert.AreEqual("greedy", ((Gadget)row[0]!).Constructor);
            Assert.AreSame(row[0], row[1]);
        }
    }
}
