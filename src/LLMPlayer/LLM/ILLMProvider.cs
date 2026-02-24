using System.Threading.Tasks;

namespace LLMPlayer.LLM
{
    public interface ILLMProvider
    {
        Task<string> GetResponseAsync(byte[] imageBytes, string systemPrompt, string userMessage);
        Task<bool> CheckHealthAsync();
        bool ValidateConfig(out string error);
    }
}
