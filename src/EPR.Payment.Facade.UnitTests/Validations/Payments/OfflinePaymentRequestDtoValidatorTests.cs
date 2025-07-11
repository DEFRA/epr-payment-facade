using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Validations.Payments;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations.Payments
{
    [TestClass]
    public class OfflinePaymentRequestDtoValidatorTests
    {
        private OfflinePaymentRequestDtoValidator _validator = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new OfflinePaymentRequestDtoValidator();
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_UserId_Is_Valid()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestDto { UserId = Guid.NewGuid(), Reference = "Test Reference", Amount = 100, Description = PaymentDescConstants.RegistrationFee, Regulator = RegulatorConstants.GBENG };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        [TestMethod]
        public void Should_Have_Error_When_Reference_Is_Empty()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Reference = string.Empty, UserId = Guid.NewGuid(), Amount = 100, Description = PaymentDescConstants.RegistrationFee, Regulator = RegulatorConstants.GBENG };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Reference_Is_Valid()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Reference = "Test Reference", UserId = Guid.NewGuid(), Amount = 100, Description = PaymentDescConstants.RegistrationFee, Regulator = RegulatorConstants.GBENG };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Have_Error_When_Amount_Is_Null()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Amount = null, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee, Regulator = RegulatorConstants.GBENG };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }       

        [TestMethod]
        [DataRow(-10)]
        [DataRow(0)]
        [DataRow(10)]
        public void Should_Not_Have_Error_When_Amount_Is_Valid(int Amount)
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Amount = Amount, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee, Regulator = RegulatorConstants.GBENG };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_Empty()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Regulator = string.Empty, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_NotSupported()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Regulator = "Test Regulator", Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_Valid()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Regulator = RegulatorConstants.GBENG, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Description_Is_Empty()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Regulator = RegulatorConstants.GBENG, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = string.Empty };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Should_Have_Error_When_Description_Is_NotSupported()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Regulator = RegulatorConstants.GBSCT, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = "Test Description" };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }
    }
}