using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using CorporateNews.Web.Plugins;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CorporateNews.Web.Services
{
    public class SemanticKernelService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;
        private ChatHistory _chatHistory;
        private readonly ILogger<SemanticKernelService> _logger;

        public SemanticKernelService(IConfiguration configuration, KernelPlugin newsPlugin, ILogger<SemanticKernelService> logger)
        {
            _logger = logger;

            var builder = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(
                    configuration["OpenAI:ModelId"] ?? "gpt-4",
                    configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey")
                );

            // Add NewsPlugin            
            builder.Plugins.Add(newsPlugin);

            _kernel = builder.Build();
            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
            _chatHistory = new ChatHistory();

            // Initialize the chat with a system message
            _chatHistory.AddSystemMessage("You are an AI assistant that helps users get information about company promotions and sales. Use the available functions to retrieve and provide relevant information.");
        }

        public async Task<string> ProcessQueryAsync(string query)
        {
            try
            {
                _logger.LogInformation($"Processing query: {query}");
                _chatHistory.AddUserMessage(query);

                var completion = _chatService.GetStreamingChatMessageContentsAsync(
                    _chatHistory,
                    executionSettings: new OpenAIPromptExecutionSettings
                    {
                        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                    },
                    kernel: _kernel);

                StringBuilder fullMessage = new StringBuilder();
                await foreach (var content in completion)
                {
                    _logger.LogDebug($"Received content: {content.Role} - {content.Content}");
                    fullMessage.Append(content.Content);
                }

                string response = fullMessage.ToString();
                _logger.LogInformation($"Final response: {response}");

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
                return $"An error occurred while processing your query: {ex.Message}";
            }
        }

        public void ResetConversation()
        {
            _chatHistory.Clear();
            _chatHistory.AddSystemMessage("You are an AI assistant that helps users get information about company promotions and sales. Use the available functions to retrieve and provide relevant information.");
        }
    }
}