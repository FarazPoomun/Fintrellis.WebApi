using Fintrellis.Services.Resiliency;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fintrellis.Services.Tests.Resiliency
{

    public class PollyRetryHandlerTests
    {
        private readonly Mock<ILogger<PollyRetryHandler>> _loggerMock;

        public PollyRetryHandlerTests()
        {
            _loggerMock = new Mock<ILogger<PollyRetryHandler>>();
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_WhenActionFails_RetriesAsExpected()
        {
            // Arrange
            int retryCount = 3;
            int incrementalCount = 1;
            var pollyRetryHandler = new PollyRetryHandler(retryCount, incrementalCount, _loggerMock.Object);

            int actionExecutionCount = 0;

            Task failingAction()
            {
                actionExecutionCount++;
                throw new InvalidOperationException("Test exception");
            }

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await pollyRetryHandler.ExecuteWithRetryAsync(failingAction));

            // Assert
            Assert.Equal(4, actionExecutionCount); 

            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((obj, t) => obj.ToString()!.Contains("Retry")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(retryCount)
            );
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_WhenActionSucceedsOnRetry_ExecutesSuccessfully()
        {
            // Arrange
            int retryCount = 3;
            int incrementalCount = 1;
            var pollyRetryHandler = new PollyRetryHandler(retryCount, incrementalCount, _loggerMock.Object);

            int actionExecutionCount = 0;

            async Task action()
            {
                actionExecutionCount++;
                if (actionExecutionCount < 2)
                {
                    throw new InvalidOperationException("Test exception");
                }
                await Task.CompletedTask;
            }

            // Act
            await pollyRetryHandler.ExecuteWithRetryAsync(action);

            // Assert
            Assert.Equal(2, actionExecutionCount);
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((obj, t) => obj.ToString()!.Contains("Retry")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once 
            );
        }
    }
}
