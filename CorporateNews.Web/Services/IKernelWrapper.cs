using Microsoft.SemanticKernel.ChatCompletion;

namespace CorporateNews.Web.Services
{
    public interface IKernelWrapper
    {
        IChatCompletionService GetChatCompletionService();
    }
}
