using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;

namespace CorporateNews.Web.Services
{
    public interface IKernelProvider
    {
        Kernel CreateKernel(string modelId, string apiKey);
        IChatCompletionService GetChatCompletionService(Kernel kernel);
    }
}
