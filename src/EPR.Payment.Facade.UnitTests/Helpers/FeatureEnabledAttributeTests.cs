using EPR.Payment.Facade.Helpers;
using FluentAssertions;

namespace EPR.Payment.Facade.UnitTests.Helpers
{
    [TestClass]
    public class FeatureEnabledAttributeTests
    {
        [TestMethod]
        public void Constructor_SetsFeatureNameProperty()
        {
            // Arrange
            var featureName = "TestFeature";

            // Act
            var attribute = new FeatureEnabledAttribute(featureName);

            // Assert
            attribute.FeatureName.Should().Be(featureName);
        }

        [TestMethod]
        public void AttributeUsage_ValidTargets()
        {
            // Arrange & Act
            var attributeUsage = Attribute.GetCustomAttribute(
                typeof(FeatureEnabledAttribute), typeof(AttributeUsageAttribute)
            ) as AttributeUsageAttribute;

            // Assert
            attributeUsage.Should().NotBeNull();
            attributeUsage!.ValidOn.Should().Be(AttributeTargets.Class | AttributeTargets.Method);
            attributeUsage.AllowMultiple.Should().BeFalse();
        }

        [TestMethod]
        public void FeatureEnabledAttribute_CanBeUsedOnClass()
        {
            // Arrange
            var type = typeof(ClassWithFeatureEnabledAttribute);

            // Act
            var attribute = Attribute.GetCustomAttribute(type, typeof(FeatureEnabledAttribute)) as FeatureEnabledAttribute;

            // Assert
            attribute.Should().NotBeNull();
            attribute!.FeatureName.Should().Be("ClassFeature");
        }

        [TestMethod]
        public void FeatureEnabledAttribute_CanBeUsedOnMethod()
        {
            // Arrange
            var method = typeof(ClassWithFeatureEnabledAttribute).GetMethod(nameof(ClassWithFeatureEnabledAttribute.MethodWithFeatureEnabledAttribute));

            // Act
            var attribute = Attribute.GetCustomAttribute(method!, typeof(FeatureEnabledAttribute)) as FeatureEnabledAttribute;

            // Assert
            attribute.Should().NotBeNull();
            attribute!.FeatureName.Should().Be("MethodFeature");
        }

        [FeatureEnabled("ClassFeature")]
        private class ClassWithFeatureEnabledAttribute
        {
            [FeatureEnabled("MethodFeature")]
            public void MethodWithFeatureEnabledAttribute() { }
        }
    }
}
