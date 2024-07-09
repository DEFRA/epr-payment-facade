using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace EPR.Payment.Facade.UnitTests.Helpers
{
    [TestClass]
    public class FeatureEnabledDocumentFilterTests
    {
        private Mock<IFeatureManager> _featureManagerMock = null!;
        private FeatureEnabledDocumentFilter _filter = null!;
        private StringWriter _output = null!;

        [TestInitialize]
        public void Setup()
        {
            _featureManagerMock = new Mock<IFeatureManager>();
            var loggerMock = new Mock<ILogger<FeatureEnabledDocumentFilter>>();
            _output = new StringWriter();
            Console.SetOut(_output);
            _filter = new FeatureEnabledDocumentFilter(_featureManagerMock.Object, loggerMock.Object);
        }

        [TestMethod]
        public void Apply_RemovesPaths_WhenActionFeatureIsDisabled()
        {
            // Arrange
            var swaggerDoc = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/test"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation>
                        {
                            [OperationType.Get] = new OpenApiOperation()
                        }
                    }
                }
            };

            var apiDescription = new ApiDescription
            {
                RelativePath = "test",
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(TestController).GetTypeInfo(),
                    EndpointMetadata = new List<object> { new FeatureGateAttribute("TestFeature") }
                }
            };

            var apiDescriptions = new List<ApiDescription> { apiDescription };
            var schemaRepository = new SchemaRepository();
            var schemaGeneratorMock = new Mock<ISchemaGenerator>();
            var context = new DocumentFilterContext(apiDescriptions, schemaGeneratorMock.Object, schemaRepository);

            _featureManagerMock.Setup(x => x.IsEnabledAsync("TestFeature")).ReturnsAsync(false);

            // Act
            _filter.Apply(swaggerDoc, context);

            // Assert
            swaggerDoc.Paths.Should().NotContainKey("/test");
            Assert.IsTrue(_output.ToString().Contains("Removing path '/test' from Swagger documentation because the feature gate is disabled."));
        }

        [TestMethod]
        public void Apply_DoesNotRemovePaths_WhenActionFeatureIsEnabled()
        {
            // Arrange
            var swaggerDoc = new OpenApiDocument
            {
                Paths = new OpenApiPaths
                {
                    ["/test"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation>
                        {
                            [OperationType.Get] = new OpenApiOperation()
                        }
                    }
                }
            };

            var apiDescription = new ApiDescription
            {
                RelativePath = "test",
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerTypeInfo = typeof(TestController).GetTypeInfo(),
                    EndpointMetadata = new List<object> { new FeatureGateAttribute("TestFeature") }
                }
            };

            var apiDescriptions = new List<ApiDescription> { apiDescription };
            var schemaRepository = new SchemaRepository();
            var schemaGeneratorMock = new Mock<ISchemaGenerator>();
            var context = new DocumentFilterContext(apiDescriptions, schemaGeneratorMock.Object, schemaRepository);

            _featureManagerMock.Setup(x => x.IsEnabledAsync("TestFeature")).ReturnsAsync(true);

            // Act
            _filter.Apply(swaggerDoc, context);

            // Assert
            swaggerDoc.Paths.Should().ContainKey("/test");
            Assert.IsFalse(_output.ToString().Contains("Removing path '/test' from Swagger documentation because the feature gate is disabled."));
        }

        private class TestController : ControllerBase
        {
        }
    }
}
