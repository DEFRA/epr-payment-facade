using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using AutoFixture.MSTest;
using EPR.Payment.Facade.Common.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace EPR.Payment.Facade.Common.UnitTests.TestHelpers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(() =>
        {
            var fixture = new Fixture().Customize(new CompositeCustomization(
                new AutoMoqCustomization { ConfigureMembers = true },
                new SupportMutableValueTypesCustomization()));

            // Configure the fixture to use the correct PaymentServiceOptions
            var optionsMock = fixture.Freeze<Mock<IOptions<OnlinePaymentServiceOptions>>>();
            optionsMock.Setup(o => o.Value).Returns(new OnlinePaymentServiceOptions
            {
                ReturnUrl = "https://example.com/return",
                ErrorUrl = "https://example.com/error"
            });

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(delegate (ThrowingRecursionBehavior b)
            {
                fixture.Behaviors.Remove((ISpecimenBuilderTransformation)(object)b);
            });
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            return fixture;
        })
        { }
    }
}