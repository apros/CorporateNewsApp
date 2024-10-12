using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;

namespace CorporateNews.Web.Services
{
    public class KernelProvider : IKernelProvider
    {
        public Kernel CreateKernel(string modelId, string apiKey)
        {
            return Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(modelId, apiKey)
                .Build();
        }

        public IChatCompletionService GetChatCompletionService(Kernel kernel)
        {
            return kernel.GetRequiredService<IChatCompletionService>();
        }
    }
}
