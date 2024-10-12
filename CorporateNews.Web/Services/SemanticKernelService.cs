using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using CorporateNews.Web.Plugins;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CorporateNews.Web.Services
{
    public class SemanticKernelService : ISemanticKernelService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;
        private readonly ILogger<SemanticKernelService> _logger;
        private readonly OpenAISettings _openAISettings;
        private ChatHistory _chatHistory;

        public SemanticKernelService(
            IOptions<OpenAISettings> openAISettings,
            KernelPlugin newsPlugin,
            ILogger<SemanticKernelService> logger,
            IKernelProvider kernelProvider)
        {
            _logger = logger;

            _kernel = kernelProvider.CreateKernel(openAISettings.Value.ModelId, openAISettings.Value.ApiKey);
            _kernel.Plugins.Add(newsPlugin);
            _chatService = kernelProvider.GetChatCompletionService(_kernel);

            ResetConversation();
        }

        protected virtual Kernel CreateKernel(OpenAISettings settings, KernelPlugin newsPlugin)
        {
            var builder = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(
                    settings.ModelId,
                    settings.ApiKey
                );

            builder.Plugins.Add(newsPlugin);

            return builder.Build();
        }

        public async Task<string> ProcessQueryAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Query cannot be null or empty", nameof(query));
            }

            _logger.LogInformation("Processing query: {Query}", query);

            try
            {
                _chatHistory.AddUserMessage(query);

                var completion = _chatService.GetStreamingChatMessageContentsAsync(
                    _chatHistory,
                    executionSettings: new OpenAIPromptExecutionSettings
                    {
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    },
                    kernel: _kernel);

                var response = await ProcessCompletionAsync(completion);

                if (string.IsNullOrWhiteSpace(response))
                {
                    _logger.LogWarning("Received empty response from AI");
                    response = "I apologize, but I couldn't generate a response. Please try asking your question again.";
                }

                _chatHistory.AddAssistantMessage(response);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing query");
                throw new SemanticKernelException("An error occurred while processing your query.", ex);
            }
        }

        public void ResetConversation()
        {
            _chatHistory = new ChatHistory();
            _chatHistory.AddSystemMessage("You are an AI assistant that helps users get information about company promotions and sales. Use the available functions to retrieve and provide relevant information.");
            _logger.LogInformation("Conversation has been reset");
        }

        private async Task<string> ProcessCompletionAsync(IAsyncEnumerable<StreamingChatMessageContent> completion)
        {
            StringBuilder fullMessage = new StringBuilder();
            await foreach (var content in completion)
            {
                _logger.LogDebug("Received content: {Role} - {Content}", content.Role, content.Content);
                fullMessage.Append(content.Content);
            }
            return fullMessage.ToString();
        }
    }

    public class OpenAISettings
    {
        public string ModelId { get; set; }
        public string ApiKey { get; set; }
    }

    public class SemanticKernelException : Exception
    {
        public SemanticKernelException(string message, Exception innerException) : base(message, innerException) { }
    }

    public interface ISemanticKernelService
    {
        Task<string> ProcessQueryAsync(string query);
        void ResetConversation();
    }
}