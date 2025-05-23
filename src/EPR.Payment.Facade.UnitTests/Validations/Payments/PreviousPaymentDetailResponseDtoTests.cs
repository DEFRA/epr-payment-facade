using EPR.Payment.Facade.Common.Dtos.Response.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Payment.Facade.UnitTests.Validations.Payments
{
    [TestClass]
    public class PreviousPaymentDetailResponseDtoTests
    {
        [TestMethod]
        public void Should_Create_Instance_With_Valid_Values()
        {
            // Arrange
            var expectedPaymentMode = "Offline";
            var expectedPaymentMethod = "BankTransfer";
            var expectedPaymentDate = new DateTime(2025, 5, 23);
            var expectedPaymentAmount = 250.00m;

            // Act
            var dto = new PreviousPaymentDetailResponseDto
            {
                PaymentMode = expectedPaymentMode,
                PaymentMethod = expectedPaymentMethod,
                PaymentDate = expectedPaymentDate,
                PaymentAmount = expectedPaymentAmount
            };

            // Assert
            Assert.AreEqual(expectedPaymentMode, dto.PaymentMode);
            Assert.AreEqual(expectedPaymentMethod, dto.PaymentMethod);
            Assert.AreEqual(expectedPaymentDate, dto.PaymentDate);
            Assert.AreEqual(expectedPaymentAmount, dto.PaymentAmount);
        }
    }

}
