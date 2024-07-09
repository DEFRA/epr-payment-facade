using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Payment.Facade.UnitTests.Helpers
{

    [TestClass]
    public class FeatureGateOperationFilterTests
    {
        private readonly Mock<IFeatureManager> _featureManagerMock;
        private readonly FeatureGateOperationFilter _filter;

        public FeatureGateOperationFilterTests()
        {
            _featureManagerMock = new Mock<IFeatureManager>();
            _filter = new FeatureGateOperationFilter(_featureManagerMock.Object);
        }

        [TestMethod]
        public void Apply_SetsDeprecated_WhenFeatureIsDisabled()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var methodInfo = typeof(TestController).GetMethod(nameof(TestController.FeatureGatedMethod));
            var context = new OperationFilterContext(
                new ApiDescription(),
                It.IsAny<ISchemaGenerator>(),
                It.IsAny<SchemaRepository>(),
                methodInfo
            );

            _featureManagerMock.Setup(x => x.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);

            // Act
            _filter.Apply(operation, context);

            // Assert
            operation.Deprecated.Should().BeTrue();
            operation.Description.Should().Contain("(This feature is currently disabled)");
        }

        [TestMethod]
        public void Apply_DoesNotSetDeprecated_WhenFeatureIsEnabled()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var methodInfo = typeof(TestController).GetMethod(nameof(TestController.FeatureGatedMethod));
            var context = new OperationFilterContext(
                new ApiDescription(),
                It.IsAny<ISchemaGenerator>(),
                It.IsAny<SchemaRepository>(),
                methodInfo
            );

            _featureManagerMock.Setup(x => x.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);

            // Act
            _filter.Apply(operation, context);

            // Assert
            operation.Deprecated.Should().BeFalse();
            operation.Description.Should().BeNullOrEmpty();
        }

        [TestMethod]
        public void Apply_DoesNothing_WhenNoFeatureGateAttributes()
        {
            // Arrange
            var operation = new OpenApiOperation();
            var methodInfo = typeof(TestController).GetMethod(nameof(TestController.NoFeatureGateMethod));
            var context = new OperationFilterContext(
                new ApiDescription(),
                It.IsAny<ISchemaGenerator>(),
                It.IsAny<SchemaRepository>(),
                methodInfo
            );

            // Act
            _filter.Apply(operation, context);

            // Assert
            operation.Deprecated.Should().BeFalse();
            operation.Description.Should().BeNullOrEmpty();
        }
    }

    [ExcludeFromCodeCoverage]
    public class TestController
    {
        [FeatureGate("TestFeature")]
        public void FeatureGatedMethod() { }

        public void NoFeatureGateMethod() { }
    }
}