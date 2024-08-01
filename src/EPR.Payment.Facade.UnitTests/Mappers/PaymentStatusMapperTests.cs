using EPR.Payment.Facade.Common.Constants;
using EPR.Payment.Facade.Common.Enums;
using EPR.Payment.Facade.Common.Mappers;
using FluentAssertions;

[TestClass]
public class PaymentStatusMapperTests
{
    [TestMethod]
    public void GetPaymentStatus_InvalidStatus_ThrowsException()
    {
        // Arrange
        string invalidStatus = "invalid_status";
        string errorCode = "P0010";

        // Act
        Action act = () => PaymentStatusMapper.GetPaymentStatus(invalidStatus, errorCode);

        // Assert
        act.Should().Throw<Exception>().WithMessage(ExceptionMessages.PaymentStatusNotFound);
    }

    [TestMethod]
    public void GetPaymentStatus_SuccessStatus_ReturnsSuccess()
    {
        // Arrange
        string status = "success";

        // Act
        var resultWithNullErrorCode = PaymentStatusMapper.GetPaymentStatus(status, null);
        var resultWithEmptyErrorCode = PaymentStatusMapper.GetPaymentStatus(status, string.Empty);

        // Assert
        resultWithNullErrorCode.Should().Be(PaymentStatus.Success);
        resultWithEmptyErrorCode.Should().Be(PaymentStatus.Success);
    }

    [TestMethod]
    public void GetPaymentStatus_FailedStatusWithP0030_ReturnsUserCancelled()
    {
        // Arrange
        string status = "failed";
        string errorCode = "P0030";

        // Act
        var result = PaymentStatusMapper.GetPaymentStatus(status, errorCode);

        // Assert
        result.Should().Be(PaymentStatus.UserCancelled);
    }

    [TestMethod]
    public void GetPaymentStatus_FailedStatusWithOtherCode_ReturnsFailed()
    {
        // Arrange
        string status = "failed";
        string errorCode = "P0010";

        // Act
        var result = PaymentStatusMapper.GetPaymentStatus(status, errorCode);

        // Assert
        result.Should().Be(PaymentStatus.Failed);
    }

    [TestMethod]
    public void GetPaymentStatus_ErrorStatusWithCode_ReturnsError()
    {
        // Arrange
        string status = "error";
        string errorCode = "P0050";

        // Act
        var result = PaymentStatusMapper.GetPaymentStatus(status, errorCode);

        // Assert
        result.Should().Be(PaymentStatus.Error);
    }

    [TestMethod]
    public void GetPaymentStatus_InvalidErrorCodeForFailedStatus_ThrowsException()
    {
        // Arrange
        string status = "failed";

        // Act & Assert
        Action actNull = () => PaymentStatusMapper.GetPaymentStatus(status, null);
        actNull.Should().Throw<Exception>().WithMessage("Error code cannot be null or empty for a failed status.");

        Action actEmpty = () => PaymentStatusMapper.GetPaymentStatus(status, string.Empty);
        actEmpty.Should().Throw<Exception>().WithMessage("Error code cannot be null or empty for a failed status.");
    }
}
