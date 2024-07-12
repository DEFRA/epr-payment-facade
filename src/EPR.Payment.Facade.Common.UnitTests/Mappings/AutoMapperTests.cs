using AutoMapper;
using EPR.Payment.Common.Mapping;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
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
        public void PaymentRequestDto_To_GovPayPaymentRequestDto_Mapping_IsValid()
        {
            var paymentRequestDto = new PaymentRequestDto
            {
                Amount = 100,
                Reference = "TestReference",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "TestRegulator"
            };

            var govPayPaymentRequestDto = _mapper.Map<GovPayPaymentRequestDto>(paymentRequestDto);

            govPayPaymentRequestDto.Amount.Should().Be(paymentRequestDto.Amount.Value);
            govPayPaymentRequestDto.Reference.Should().Be(paymentRequestDto.Reference);
            govPayPaymentRequestDto.OrganisationId.Should().Be(paymentRequestDto.OrganisationId.Value);
            govPayPaymentRequestDto.UserId.Should().Be(paymentRequestDto.UserId.Value);
            govPayPaymentRequestDto.Regulator.Should().Be(paymentRequestDto.Regulator);
            govPayPaymentRequestDto.return_url.Should().BeNull(); // Ignored in mapping
            govPayPaymentRequestDto.Description.Should().BeNull(); // Ignored in mapping
        }

        [TestMethod]
        public void PaymentRequestDto_To_InsertPaymentRequestDto_Mapping_IsValid()
        {
            var paymentRequestDto = new PaymentRequestDto
            {
                Amount = 200,
                Reference = "AnotherReference",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "AnotherRegulator"
            };

            var insertPaymentRequestDto = _mapper.Map<InsertPaymentRequestDto>(paymentRequestDto);

            insertPaymentRequestDto.Amount.Should().Be(paymentRequestDto.Amount.Value);
            insertPaymentRequestDto.Reference.Should().Be(paymentRequestDto.Reference);
            insertPaymentRequestDto.OrganisationId.Should().Be(paymentRequestDto.OrganisationId.Value);
            insertPaymentRequestDto.UserId.Should().Be(paymentRequestDto.UserId.Value);
            insertPaymentRequestDto.Regulator.Should().Be(paymentRequestDto.Regulator);
            insertPaymentRequestDto.ReasonForPayment.Should().BeNull(); // Ignored in mapping
            insertPaymentRequestDto.Status.Should().Be(0); // Default value for enum, ignored in mapping
        }

        [TestMethod]
        public void PaymentRequestDto_To_UpdatePaymentRequestDto_Mapping_IsValid()
        {
            var paymentRequestDto = new PaymentRequestDto
            {
                Amount = 300,
                Reference = "UpdateReference",
                OrganisationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Regulator = "UpdateRegulator"
            };

            var updatePaymentRequestDto = _mapper.Map<UpdatePaymentRequestDto>(paymentRequestDto);

            updatePaymentRequestDto.Reference.Should().Be(paymentRequestDto.Reference);
            updatePaymentRequestDto.UpdatedByOrganisationId.Should().Be(paymentRequestDto.OrganisationId.Value);
            updatePaymentRequestDto.UpdatedByUserId.Should().Be(paymentRequestDto.UserId.Value);
            updatePaymentRequestDto.Status.Should().Be(0); // Default value for enum, ignored in mapping
            updatePaymentRequestDto.GovPayPaymentId.Should().BeNull(); // Ignored in mapping
            updatePaymentRequestDto.ErrorCode.Should().BeNull(); // Ignored in mapping
        }

        [TestMethod]
        public void CompletePaymentRequestDto_To_UpdatePaymentRequestDto_Mapping_IsValid()
        {
            var completePaymentRequestDto = new CompletePaymentRequestDto
            {
                Id = Guid.NewGuid(),
                UpdatedByUserId = Guid.NewGuid(),
                UpdatedByOrganisationId = Guid.NewGuid()
            };

            var updatePaymentRequestDto = _mapper.Map<UpdatePaymentRequestDto>(completePaymentRequestDto);

            updatePaymentRequestDto.Id.Should().Be(completePaymentRequestDto.Id);
            updatePaymentRequestDto.UpdatedByUserId.Should().Be(completePaymentRequestDto.UpdatedByUserId);
            updatePaymentRequestDto.UpdatedByOrganisationId.Should().Be(completePaymentRequestDto.UpdatedByOrganisationId);
            updatePaymentRequestDto.Reference.Should().BeNull(); // Ignored in mapping
            updatePaymentRequestDto.Status.Should().Be(0); // Default value for enum, ignored in mapping
            updatePaymentRequestDto.ErrorCode.Should().BeNull(); // Ignored in mapping
            updatePaymentRequestDto.GovPayPaymentId.Should().BeNull(); // Ignored in mapping
        }
    }
}
