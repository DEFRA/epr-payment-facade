using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments.V2Payments;
using EPR.Payment.Facade.Common.Enums.Payments;
using EPR.Payment.Facade.Validations.Payments;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations.Payments
{
    [TestClass]
    public class OnlinePaymentRequestDtoV2ValidatorTests
    {
        private OnlinePaymentRequestDtoV2Validator _validator = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new OnlinePaymentRequestDtoV2Validator();
        }

        [TestMethod]
        public void Should_Have_Error_When_UserId_Is_Null()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { UserId = null, Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_UserId_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        [TestMethod]
        public void Should_Have_Error_When_OrganisationId_Is_Null()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { OrganisationId = null, Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.OrganisationId);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_OrganisationId_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { OrganisationId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.OrganisationId);
        }

        [TestMethod]
        public void Should_Have_Error_When_Reference_Is_Empty()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Reference = string.Empty, Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Reference_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Reference = "Test Reference", Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_Empty()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Regulator = string.Empty, Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_NotSupported()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Regulator = RegulatorConstants.GBSCT, Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_ReasonForPayment_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Regulator = RegulatorConstants.GBENG, Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Amount_Is_Null()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Amount = null, Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void Should_Have_Error_When_Amount_Is_Less_Then_One()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Amount = 0, Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Amount_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Amount = 10, Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void Should_Have_Error_When_Description_Is_Null()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Description = "null" };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Should_Have_Error_When_Description_Is_Empty()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Description = "" };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Should_Have_Error_When_Description_Is_Not_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Description = "Test Description" };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Description_Is_RegistrationFee()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Description_Is_PackagingResubmissionFee()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Description = PaymentDescConstants.PackagingResubmissionFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void ShouldNot_Have_Error_When_RequestorType_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestV2Dto { Regulator = RegulatorConstants.GBSCT, Description = PaymentDescConstants.RegistrationFee, RequestorType = PaymentsRequestorTypes.Reprocessors, Amount =20, OrganisationId = Guid.NewGuid(), Reference = "901AB9E3-F952-4467-87FF-389EF93E7E95", UserId = Guid.NewGuid() };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.RequestorType);
        }
    }
}
