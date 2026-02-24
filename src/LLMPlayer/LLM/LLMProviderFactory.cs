using LLMPlayer.LLM.Providers;
using System;

namespace LLMPlayer.LLM
{
    public static class LLMProviderFactory
    {
        /// <summary>
        /// Creates an ILLMProvider instance based on the current plugin configuration.
        /// </summary>
        /// <returns>An ILLMProvider configured according to Plugin.Instance.ProviderType and its related settings.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when Plugin.Instance.ProviderType has an unsupported value.</exception>
        public static ILLMProvider CreateProvider()
        {
            var config = Plugin.Instance;
            switch (config.ProviderType.Value)
            {
                case LLMProviderType.Ollama:
                    return new OllamaProvider(config.OllamaEndpoint.Value, config.OllamaModel.Value);
                case LLMProviderType.OpenAICompatible:
                    return new OpenAICompatibleProvider(config.OpenAIEndpoint.Value, config.OpenAIKey.Value, config.OpenAIModel.Value);
                case LLMProviderType.Kobold:
                    return new KoboldProvider(config.KoboldEndpoint.Value, config.KoboldModel.Value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(config.ProviderType), "Unsupported LLM provider type.");
            }
        }
    }
}
