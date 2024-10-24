﻿using EPR.Payment.Facade.Common.Constants;
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
        public void Should_Have_Error_When_UserId_Is_Null()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestDto { UserId = null };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_UserId_Is_Valid()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestDto { UserId = Guid.NewGuid() };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.UserId);
        }

        [TestMethod]
        public void Should_Have_Error_When_Reference_Is_Empty()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Reference = string.Empty };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Reference_Is_Valid()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Reference = "Test Reference" };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Reference);
        }

        [TestMethod]
        public void Should_Have_Error_When_Amount_Is_Null()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Amount = null };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void Should_Not_Have_Error_When_Amount_Is_Valid()
        {
            var offlinePaymentStatusInsertRequestDto = new OfflinePaymentRequestDto { Amount = 10 };
            var result = _validator.TestValidate(offlinePaymentStatusInsertRequestDto);
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }
        //TODO: validation for other fields
    }
}