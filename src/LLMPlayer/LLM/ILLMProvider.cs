using System.Threading;
using System.Threading.Tasks;

namespace LLMPlayer.LLM
{
    public interface ILLMProvider
    {
        Task<string> GetResponseAsync(byte[] imageBytes, string systemPrompt, string userMessage, CancellationToken cancellationToken);
        Task<bool> CheckHealthAsync(CancellationToken cancellationToken);
        bool ValidateConfig(out string error);
    }
}
