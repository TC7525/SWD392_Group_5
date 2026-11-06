using System;
using Xunit;
using backend.Common;
using backend.Enums;
using backend.Services;

namespace backend.Tests
{
    public class POStatusServiceTests
    {
        private readonly IPOStatusService _service;

        public POStatusServiceTests()
        {
            _service = new POStatusService();
        }

        [Theory]
        [InlineData(POStatus.Pending, POStatus.Approved)]
        [InlineData(POStatus.Pending, POStatus.Rejected)]
        [InlineData(POStatus.Approved, POStatus.Processing)]
        [InlineData(POStatus.PartiallyReceived, POStatus.Processing)]
        [InlineData(POStatus.Processing, POStatus.PartiallyReceived)]
        [InlineData(POStatus.Processing, POStatus.Completed)]
        [InlineData(POStatus.Processing, POStatus.Approved)]
        public void CanTransition_ValidTransitions_ReturnsTrue(POStatus current, POStatus target)
        {
            // Act
            var result = _service.CanTransition(current, target);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(POStatus.Pending, POStatus.Approved)]
        [InlineData(POStatus.Pending, POStatus.Rejected)]
        [InlineData(POStatus.Approved, POStatus.Processing)]
        [InlineData(POStatus.PartiallyReceived, POStatus.Processing)]
        [InlineData(POStatus.Processing, POStatus.PartiallyReceived)]
        [InlineData(POStatus.Processing, POStatus.Completed)]
        [InlineData(POStatus.Processing, POStatus.Approved)]
        public void ValidateTransition_ValidTransitions_DoesNotThrow(POStatus current, POStatus target)
        {
            // Act & Assert
            var exception = Record.Exception(() => _service.ValidateTransition(current, target));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(POStatus.Completed, POStatus.Processing)]
        [InlineData(POStatus.Rejected, POStatus.Approved)]
        [InlineData(POStatus.Pending, POStatus.Completed)]
        [InlineData(POStatus.Pending, POStatus.Processing)]
        [InlineData(POStatus.Approved, POStatus.Completed)]
        [InlineData(POStatus.Approved, POStatus.Approved)]
        [InlineData(POStatus.Completed, POStatus.Completed)]
        [InlineData(POStatus.Rejected, POStatus.Pending)]
        [InlineData(POStatus.PartiallyReceived, POStatus.Completed)]
        public void CanTransition_InvalidTransitions_ReturnsFalse(POStatus current, POStatus target)
        {
            // Act
            var result = _service.CanTransition(current, target);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(POStatus.Completed, POStatus.Processing)]
        [InlineData(POStatus.Rejected, POStatus.Approved)]
        [InlineData(POStatus.Pending, POStatus.Completed)]
        [InlineData(POStatus.Pending, POStatus.Processing)]
        [InlineData(POStatus.Approved, POStatus.Completed)]
        [InlineData(POStatus.PartiallyReceived, POStatus.Completed)]
        public void ValidateTransition_InvalidTransitions_ThrowsAppExceptionWithMSG16(POStatus current, POStatus target)
        {
            // Act & Assert
            var ex = Assert.Throws<AppException>(() => _service.ValidateTransition(current, target));
            Assert.Equal("MSG16", ex.Code);
            Assert.Equal(400, ex.StatusCode);
            Assert.Equal("The requested status transition is not allowed.", ex.Message);
        }
    }
}
