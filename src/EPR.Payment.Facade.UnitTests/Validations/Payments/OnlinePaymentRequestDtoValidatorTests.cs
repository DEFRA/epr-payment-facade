using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Validations.Payments;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations.Payments
{
    [TestClass]
    public class OnlinePaymentRequestDtoValidatorTests
    {
        private OnlinePaymentRequestDtoValidator _validator = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new OnlinePaymentRequestDtoValidator();
        }

        [TestMethod]
        public void Should_Have_Error_When_UserId_Is_Null()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { UserId = null, Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_UserId_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { UserId = Guid.NewGuid(), Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        [TestMethod]
        public void Should_Have_Error_When_OrganisationId_Is_Null()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { OrganisationId = null, Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.OrganisationId);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_OrganisationId_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { OrganisationId = Guid.NewGuid(), Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.OrganisationId);
        }

        [TestMethod]
        public void Should_Have_Error_When_Reference_Is_Empty()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Reference = string.Empty, Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Reference_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Reference = "Test Reference", Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_Empty()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Regulator = string.Empty, Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_NotSupported()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Regulator = RegulatorConstants.GBSCT, Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_ReasonForPayment_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Regulator = RegulatorConstants.GBENG, Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Amount_Is_Null()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Amount = null, Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void Should_Have_Error_When_Amount_Is_Less_Then_One()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Amount = 0, Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Amount_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Amount = 10, Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void Should_Have_Error_When_Description_Is_Null()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Description = "null" };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Should_Have_Error_When_Description_Is_Empty()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Description = "" };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Should_Have_Error_When_Description_Is_Not_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Description = "Test Description" };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Description_Is_RegistrationFee()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Description = OfflinePayDescConstants.RegistrationFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Description_Is_PackagingResubmissionFee()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Description = OfflinePayDescConstants.PackagingResubmissionFee };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }
    }
}
