using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace CorporateNews.Web.Services
{
    public class KernelWrapper : IKernelWrapper
    {
        private readonly Kernel _kernel;

        public KernelWrapper(Kernel kernel)
        {
            _kernel = kernel;
        }

        public IChatCompletionService GetChatCompletionService()
        {
            return _kernel.GetRequiredService<IChatCompletionService>();
        }
    }
}
