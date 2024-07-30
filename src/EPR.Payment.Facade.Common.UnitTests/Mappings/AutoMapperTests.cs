using AutoFixture;
using AutoMapper;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Enums;
using FluentAssertions;

namespace EPR.Payment.Facade.Common.UnitTests.Mappings
{
    [TestClass]
    public class AutoMapperTests
    {
        private readonly IMapper _mapper;

        public AutoMapperTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentRequestMappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        [TestMethod]
        public void AutoMapper_Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentRequestMappingProfile>();
            });

            Action configAction = () => config.AssertConfigurationIsValid();
            configAction.Should().NotThrow();
        }

        [TestMethod]
        public void PaymentRequestDto_To_GovPayRequestDto_Mapping_IsValid()
        {
            var fixture = new Fixture();
            var paymentRequestDto = fixture.Create<PaymentRequestDto>();
            var govPayRequestDto = _mapper.Map<GovPayRequestDto>(paymentRequestDto);

            using (new FluentAssertions.Execution.AssertionScope())
            {
                if (paymentRequestDto.Amount.HasValue)
                {
                    govPayRequestDto.Amount.Should().Be(paymentRequestDto.Amount.Value);
                }

                govPayRequestDto.Reference.Should().Be(paymentRequestDto.Reference);

                if (paymentRequestDto.OrganisationId.HasValue)
                {
                    govPayRequestDto.OrganisationId.Should().Be(paymentRequestDto.OrganisationId.Value);
                }

                if (paymentRequestDto.UserId.HasValue)
                {
                    govPayRequestDto.UserId.Should().Be(paymentRequestDto.UserId.Value);
                }

                govPayRequestDto.Regulator.Should().Be(paymentRequestDto.Regulator);
                govPayRequestDto.return_url.Should().BeNull(); // Ignored in mapping
                govPayRequestDto.Description.Should().BeNull(); // Ignored in mapping
            }
        }

        [TestMethod]
        public void PaymentRequestDto_To_InsertPaymentRequestDto_Mapping_IsValid()
        {
            var fixture = new Fixture();
            var paymentRequestDto = fixture.Create<PaymentRequestDto>();
            var insertPaymentRequestDto = _mapper.Map<InsertPaymentRequestDto>(paymentRequestDto);

            using (new FluentAssertions.Execution.AssertionScope())
            {
                if (paymentRequestDto.Amount.HasValue)
                {
                    insertPaymentRequestDto.Amount.Should().Be(paymentRequestDto.Amount.Value);
                }

                insertPaymentRequestDto.Reference.Should().Be(paymentRequestDto.Reference);

                if (paymentRequestDto.OrganisationId.HasValue)
                {
                    insertPaymentRequestDto.OrganisationId.Should().Be(paymentRequestDto.OrganisationId.Value);
                }

                if (paymentRequestDto.UserId.HasValue)
                {
                    insertPaymentRequestDto.UserId.Should().Be(paymentRequestDto.UserId.Value);
                }

                insertPaymentRequestDto.Regulator.Should().Be(paymentRequestDto.Regulator);
                insertPaymentRequestDto.ReasonForPayment.Should().BeNull(); // Ignored in mapping
                insertPaymentRequestDto.Status.Should().Be(PaymentStatus.Initiated); // Default value for enum, ignored in mapping
            }
        }

        [TestMethod]
        public void PaymentRequestDto_To_UpdatePaymentRequestDto_Mapping_IsValid()
        {
            var fixture = new Fixture();
            var paymentRequestDto = fixture.Create<PaymentRequestDto>();
            var updatePaymentRequestDto = _mapper.Map<UpdatePaymentRequestDto>(paymentRequestDto);

            using (new FluentAssertions.Execution.AssertionScope())
            {
                updatePaymentRequestDto.Reference.Should().Be(paymentRequestDto.Reference);

                if (paymentRequestDto.OrganisationId.HasValue)
                {
                    updatePaymentRequestDto.UpdatedByOrganisationId.Should().Be(paymentRequestDto.OrganisationId.Value);
                }

                if (paymentRequestDto.UserId.HasValue)
                {
                    updatePaymentRequestDto.UpdatedByUserId.Should().Be(paymentRequestDto.UserId.Value);
                }

                updatePaymentRequestDto.Status.Should().Be(PaymentStatus.InProgress); // Default value for enum, ignored in mapping
                updatePaymentRequestDto.GovPayPaymentId.Should().BeNull(); // Ignored in mapping
                updatePaymentRequestDto.ErrorCode.Should().BeNull(); // Ignored in mapping
                updatePaymentRequestDto.ErrorMessage.Should().BeNull(); // Ignored in mapping
            }
        }
    }
}
