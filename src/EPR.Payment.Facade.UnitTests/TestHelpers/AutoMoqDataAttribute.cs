using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using AutoFixture.MSTest;
using AutoMapper;
using EPR.Payment.Common.Mapping;
using EPR.Payment.Facade.Common.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Moq;

namespace EPR.Payment.Facade.UnitTests.TestHelpers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(() =>
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

            // Configure the fixture to use the correct PaymentServiceOptions
            var optionsMock = fixture.Freeze<Mock<IOptions<PaymentServiceOptions>>>();
            optionsMock.Setup(o => o.Value).Returns(new PaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                Description = "Payment description"
            });

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentRequestMappingProfile>();
            });
            var mapper = mapperConfig.CreateMapper();
            fixture.Inject(mapper);

            // Remove the default recursion behavior
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));

            // Add behavior to omit on recursion
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Add customizations to handle MVC-related properties
            fixture.Customizations.Add(
                new RecursionGuard(
                    new TypeRelay(
                        typeof(ControllerContext),
                        typeof(ControllerContext)),
                    new OmitOnRecursionHandler()));

            fixture.Customizations.Add(
                new RecursionGuard(
                    new TypeRelay(
                        typeof(ControllerActionDescriptor),
                        typeof(ControllerActionDescriptor)),
                    new OmitOnRecursionHandler()));

            return fixture;
        })
        {
        }
    }
}
