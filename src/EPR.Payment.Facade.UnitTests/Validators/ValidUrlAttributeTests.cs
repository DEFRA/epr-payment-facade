using EPR.Payment.Facade.Common.Validators;
using FluentAssertions;

namespace EPR.Payment.Facade.UnitTests.Validators
{
    [TestClass]
    public class ValidUrlAttributeTests
    {
        private ValidUrlAttribute? _attribute;

        [TestInitialize]
        public void Setup()
        {
            _attribute = new ValidUrlAttribute();
        }

        [TestMethod]
        public void IsValid_ValidUrl_ReturnsTrue()
        {
            var result = _attribute?.IsValid("https://example.com");
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsValid_InvalidUrl_ReturnsFalse()
        {
            var result = _attribute?.IsValid("invalid_url");
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsValid_NullOrEmptyUrl_ReturnsFalse()
        {
            var result = _attribute?.IsValid(null);
            result.Should().BeFalse();

            result = _attribute?.IsValid(string.Empty);
            result.Should().BeFalse();
        }
    }
}
