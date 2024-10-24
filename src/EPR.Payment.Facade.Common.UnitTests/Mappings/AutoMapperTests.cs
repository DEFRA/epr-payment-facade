using AutoMapper;
using EPR.Payment.Common.Mapping;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.UnitTests.TestHelpers;
using FluentAssertions;
using FluentAssertions.Execution;

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

        [TestMethod, AutoMoqData]
        public void AutoMapper_Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PaymentRequestMappingProfile>();
            });

            Action configAction = () => config.AssertConfigurationIsValid();
            configAction.Should().NotThrow();
        }

        [TestMethod, AutoMoqData]
        public void PaymentRequestDto_To_GovPayRequestDto_Mapping_IsValid(OnlinePaymentRequestDto onlinePaymentRequestDto)
        {
            var govPayRequestDto = _mapper.Map<GovPayRequestDto>(onlinePaymentRequestDto);

            using (new AssertionScope())
            {
                if (onlinePaymentRequestDto.Amount.HasValue)
                {
                    govPayRequestDto.Amount.Should().Be(onlinePaymentRequestDto.Amount.Value);
                }

                govPayRequestDto.Reference.Should().Be(onlinePaymentRequestDto.Reference);

                if (onlinePaymentRequestDto.OrganisationId.HasValue)
                {
                    govPayRequestDto.OrganisationId.Should().Be(onlinePaymentRequestDto.OrganisationId.Value);
                }

                if (onlinePaymentRequestDto.UserId.HasValue)
                {
                    govPayRequestDto.UserId.Should().Be(onlinePaymentRequestDto.UserId.Value);
                }

                govPayRequestDto.Regulator.Should().Be(onlinePaymentRequestDto.Regulator);
                govPayRequestDto.return_url.Should().BeNull(); // Ignored in mapping
                govPayRequestDto.Description.Should().BeNull(); // Ignored in mapping
            }
        }

        [TestMethod, AutoMoqData]
        public void PaymentRequestDto_To_InsertPaymentRequestDto_Mapping_IsValid(OnlinePaymentRequestDto onlinePaymentRequestDto)
        {
            var insertOnlinePaymentRequestDto = _mapper.Map<InsertOnlinePaymentRequestDto>(onlinePaymentRequestDto);

            using (new AssertionScope())
            {
                if (onlinePaymentRequestDto.Amount.HasValue)
                {
                    insertOnlinePaymentRequestDto.Amount.Should().Be(onlinePaymentRequestDto.Amount.Value);
                }

                insertOnlinePaymentRequestDto.Reference.Should().Be(onlinePaymentRequestDto.Reference);

                if (onlinePaymentRequestDto.OrganisationId.HasValue)
                {
                    insertOnlinePaymentRequestDto.OrganisationId.Should().Be(onlinePaymentRequestDto.OrganisationId.Value);
                }

                if (onlinePaymentRequestDto.UserId.HasValue)
                {
                    insertOnlinePaymentRequestDto.UserId.Should().Be(onlinePaymentRequestDto.UserId.Value);
                }

                insertOnlinePaymentRequestDto.Regulator.Should().Be(onlinePaymentRequestDto.Regulator);
                insertOnlinePaymentRequestDto.ReasonForPayment.Should().BeNull(); // Ignored in mapping
                insertOnlinePaymentRequestDto.Status.Should().Be(PaymentStatus.Initiated); // Default value for enum, ignored in mapping
            }
        }

        [TestMethod, AutoMoqData]
        public void PaymentRequestDto_To_UpdatePaymentRequestDto_Mapping_IsValid(OnlinePaymentRequestDto onlinePaymentRequestDto)
        {
            var updateOnlinePaymentRequestDto = _mapper.Map<UpdateOnlinePaymentRequestDto>(onlinePaymentRequestDto);

            using (new AssertionScope())
            {
                updateOnlinePaymentRequestDto.Reference.Should().Be(onlinePaymentRequestDto.Reference);

                if (onlinePaymentRequestDto.OrganisationId.HasValue)
                {
                    updateOnlinePaymentRequestDto.UpdatedByOrganisationId.Should().Be(onlinePaymentRequestDto.OrganisationId.Value);
                }

                if (onlinePaymentRequestDto.UserId.HasValue)
                {
                    updateOnlinePaymentRequestDto.UpdatedByUserId.Should().Be(onlinePaymentRequestDto.UserId.Value);
                }

                updateOnlinePaymentRequestDto.Status.Should().Be(PaymentStatus.InProgress); // Default value for enum, ignored in mapping
                updateOnlinePaymentRequestDto.GovPayPaymentId.Should().BeNull(); // Ignored in mapping
                updateOnlinePaymentRequestDto.ErrorCode.Should().BeNull(); // Ignored in mapping
                updateOnlinePaymentRequestDto.ErrorMessage.Should().BeNull(); // Ignored in mapping
            }
        }
    }
}
