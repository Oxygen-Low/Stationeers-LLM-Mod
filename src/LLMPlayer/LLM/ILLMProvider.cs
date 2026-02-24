using System.Threading.Tasks;

namespace LLMPlayer.LLM
{
    public interface ILLMProvider
    {
        /// <summary>
/// Generates a textual response from the provider using the provided image data and prompt context.
/// </summary>
/// <param name="imageBytes">Raw image data as a byte array to be used as input context.</param>
/// <param name="systemPrompt">A system-level prompt that establishes context or instructions for the provider.</param>
/// <param name="userMessage">The user's message or query to which the provider should respond.</param>
/// <returns>The generated response text from the provider.</returns>
Task<string> GetResponseAsync(byte[] imageBytes, string systemPrompt, string userMessage);
        /// <summary>
/// Performs a health check of the LLM provider.
/// </summary>
/// <returns>True if the provider is healthy and ready to process requests, false otherwise.</returns>
Task<bool> CheckHealthAsync();
        /// <summary>
/// Validates the provider's configuration and reports any validation error.
/// </summary>
/// <param name="error">When validation fails, contains a descriptive error message; otherwise null or empty.</param>
/// <returns>`true` if the configuration is valid, `false` otherwise.</returns>
bool ValidateConfig(out string error);
    }
}
