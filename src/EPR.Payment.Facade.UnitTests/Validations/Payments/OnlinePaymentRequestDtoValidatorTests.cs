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
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { UserId = null };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_UserId_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { UserId = Guid.NewGuid() };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        [TestMethod]
        public void Should_Have_Error_When_OrganisationId_Is_Null()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { OrganisationId = null };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.OrganisationId);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_OrganisationId_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { OrganisationId = Guid.NewGuid() };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.OrganisationId);
        }

        [TestMethod]
        public void Should_Have_Error_When_Reference_Is_Empty()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Reference = string.Empty };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Reference_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Reference = "Test Reference" };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_Empty()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Regulator = string.Empty };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_NotSupported()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Regulator = RegulatorConstants.GBSCT };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_ReasonForPayment_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Regulator = RegulatorConstants.GBENG };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Amount_Is_Null()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Amount = null };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void Should_Have_Error_When_Amount_Is_Less_Then_One()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Amount = 0 };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Amount_Is_Valid()
        {
            var onlinePaymentStatusInsertRequestDto = new OnlinePaymentRequestDto { Amount = 10 };
            var result = _validator.TestValidate(onlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }
    }
}
