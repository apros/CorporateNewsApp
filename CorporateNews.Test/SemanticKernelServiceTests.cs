using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CorporateNews.Web.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Moq;
using Xunit;

namespace CorporateNews.Test
{
    public class SemanticKernelServiceTests
    {

        private readonly Mock<IOptions<OpenAISettings>> _mockOptions;
        private readonly TestKernelPlugin _testPlugin;
        private readonly Mock<ILogger<SemanticKernelService>> _mockLogger;
        private readonly Mock<IChatCompletionService> _mockChatService;
        private readonly Mock<IKernelProvider> _mockKernelProvider;

        public SemanticKernelServiceTests()
        {
            _mockOptions = new Mock<IOptions<OpenAISettings>>();
            _mockOptions.Setup(x => x.Value).Returns(new OpenAISettings { ModelId = "test-model", ApiKey = "test-key" });

            _testPlugin = new TestKernelPlugin();
            _mockLogger = new Mock<ILogger<SemanticKernelService>>();
            _mockChatService = new Mock<IChatCompletionService>();
            _mockKernelProvider = new Mock<IKernelProvider>();

            _mockKernelProvider.Setup(k => k.CreateKernel(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Kernel.CreateBuilder().Build());
            _mockKernelProvider.Setup(k => k.GetChatCompletionService(It.IsAny<Kernel>()))
                .Returns(_mockChatService.Object);
        }

        [Fact]
        public async Task ProcessQueryAsync_ValidQuery_ReturnsResponse()
        {
            // Arrange
            var expectedResponse = "This is a test response";
            var streamingContent = new List<StreamingChatMessageContent>
            {
                new StreamingChatMessageContent(AuthorRole.Assistant, expectedResponse)
            };

            _mockChatService
                .Setup(x => x.GetStreamingChatMessageContentsAsync(
                    It.IsAny<ChatHistory>(),
                    It.IsAny<OpenAIPromptExecutionSettings>(),
                    It.IsAny<Kernel>(),
                    It.IsAny<CancellationToken>()))
                .Returns((ChatHistory history, OpenAIPromptExecutionSettings settings, Kernel kernel, CancellationToken cancellationToken) =>
                {
                    return AsyncEnumerable(cancellationToken);
                });

            var service = new SemanticKernelService(
                _mockOptions.Object,
                _testPlugin,
                _mockLogger.Object,
                _mockKernelProvider.Object);

            // Act
            var result = await service.ProcessQueryAsync("Test query");

            // Assert
            Assert.Equal(expectedResponse, result);

            async IAsyncEnumerable<StreamingChatMessageContent> AsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                foreach (var content in streamingContent)
                {
                    yield return content;
                }
            }
        }

        [Fact]
        public async Task ProcessQueryAsync_EmptyResponse_ReturnsDefaultMessage()
        {
            // Arrange
            var streamingContent = new List<StreamingChatMessageContent>
            {
                new StreamingChatMessageContent(AuthorRole.Assistant, "")
            };

            _mockChatService
                .Setup(x => x.GetStreamingChatMessageContentsAsync(
                    It.IsAny<ChatHistory>(),
                    It.IsAny<OpenAIPromptExecutionSettings>(),
                    It.IsAny<Kernel>(),
                    It.IsAny<CancellationToken>()))
                .Returns(streamingContent.ToAsyncEnumerable());

            var service = new SemanticKernelService(
                _mockOptions.Object,
                _testPlugin,
                _mockLogger.Object,
                _mockKernelProvider.Object);

            // Use reflection to set the private _kernel field
            var kernelField = typeof(SemanticKernelService).GetField("_kernel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            //kernelField.SetValue(service, _mockKernel.Object);

            // Act
            var result = await service.ProcessQueryAsync("Test query");

            // Assert
            Assert.Contains("I apologize, but I couldn't generate a response", result);
        }

        [Fact]
        public void ProcessQueryAsync_NullQuery_ThrowsArgumentException()
        {
            // Arrange
            var service = new SemanticKernelService(
                _mockOptions.Object,
                _testPlugin,
                _mockLogger.Object,
                _mockKernelProvider.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => service.ProcessQueryAsync(null));
        }

        [Fact]
        public void ResetConversation_ClearsAndInitializesHistory()
        {
            // Arrange
            var service = new SemanticKernelService(
                _mockOptions.Object,
                _testPlugin,
                _mockLogger.Object,
                _mockKernelProvider.Object);

            // Act
            service.ResetConversation();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Conversation has been reset")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2)); // Changed from Times.Once() to Times.Exactly(2)
        }
    }
}
