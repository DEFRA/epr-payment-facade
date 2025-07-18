﻿using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Dtos.Request.Payments;
using EPR.Payment.Facade.Validations.Payments;
using EPR.Payment.Facade.Common.Enums.Payments;
using FluentValidation.TestHelper;

namespace EPR.Payment.Facade.UnitTests.Validations.Payments
{
    [TestClass]
    public class OfflinePaymentRequestV2DtoValidatorTests
    {
        private OfflinePaymentRequestV2DtoValidator _validator = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new OfflinePaymentRequestV2DtoValidator();
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_UserId_Is_Valid()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { UserId = Guid.NewGuid(), Reference = "Test Reference", Amount = 100, Description = PaymentDescConstants.RegistrationFee, Regulator = RegulatorConstants.GBENG, PaymentMethod = OfflinePaymentMethodTypes.Cheque };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        [TestMethod]
        public void Should_Have_Error_When_Reference_Is_Empty()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Reference = string.Empty, UserId = Guid.NewGuid(), Amount = 100, Description = PaymentDescConstants.RegistrationFee, Regulator = RegulatorConstants.GBENG, PaymentMethod = OfflinePaymentMethodTypes.Cheque };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Reference_Is_Valid()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Reference = "Test Reference", UserId = Guid.NewGuid(), Amount = 100, Description = PaymentDescConstants.RegistrationFee, Regulator = RegulatorConstants.GBENG, PaymentMethod = OfflinePaymentMethodTypes.Cheque };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Amount_Is_Valid()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee, Regulator = RegulatorConstants.GBENG, PaymentMethod = OfflinePaymentMethodTypes.Cheque };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_Empty()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Regulator = string.Empty, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee, PaymentMethod = OfflinePaymentMethodTypes.Cheque };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_NotSupported()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Regulator = "Test Regulator", Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee, PaymentMethod = OfflinePaymentMethodTypes.Cheque };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Regulator_Is_Valid()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Regulator = RegulatorConstants.GBENG, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee, PaymentMethod = OfflinePaymentMethodTypes.Cheque };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Regulator);
        }

        [TestMethod]
        public void Should_Have_Error_When_Description_Is_Empty()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Regulator = RegulatorConstants.GBENG, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = string.Empty, PaymentMethod = OfflinePaymentMethodTypes.Cheque };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Should_Have_Error_When_Description_Is_NotSupported()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Regulator = RegulatorConstants.GBSCT, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = "Test Description", PaymentMethod = OfflinePaymentMethodTypes.Cheque };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [TestMethod]
        public void Should_Have_Error_When_PaymentMethod_Is_NotSupplied()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Regulator = RegulatorConstants.GBENG, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = "Test Description" };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.PaymentMethod);
        }

        [TestMethod]
        public void ShouldNot_Have_Error_When_PaymentMethod_Is_Valid()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Regulator = RegulatorConstants.GBENG, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee, PaymentMethod = OfflinePaymentMethodTypes.Cheque };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.PaymentMethod);
        }

        [TestMethod]
        public void ShouldNot_Have_Error_When_OrganisationId_Is_Valid()
        {
            var paymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Regulator = RegulatorConstants.GBENG, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = PaymentDescConstants.RegistrationFee, PaymentMethod = OfflinePaymentMethodTypes.Cheque, OrganisationId=Guid.NewGuid() };
            var result = _validator.TestValidate(paymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.OrganisationId);
        }

        [TestMethod]
        public void Should_Have_Error_When_OrganisationId_Is_EmptyGuid()
        {

            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Regulator = RegulatorConstants.GBENG, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = "Test Description", OrganisationId = Guid.Empty };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.OrganisationId);
        }

        [TestMethod]
        public void Should_Have_Error_When_OrganisationId_Is_Null()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestV2Dto { Regulator = RegulatorConstants.GBENG, Amount = 10, Reference = "Test Reference", UserId = Guid.NewGuid(), Description = "Test Description", OrganisationId = null };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.OrganisationId);
        }
    }
}