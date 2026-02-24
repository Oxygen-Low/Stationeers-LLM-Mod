using LLMPlayer.LLM.Providers;
using System;

namespace LLMPlayer.LLM
{
    public static class LLMProviderFactory
    {
        public static ILLMProvider CreateProvider()
        {
            var config = Plugin.Instance;
            switch (config.ProviderType.Value)
            {
                case LLMProviderType.Ollama:
                    if (string.IsNullOrWhiteSpace(config.OllamaEndpoint.Value))
                        throw new ArgumentException("Ollama endpoint is empty");
                    if (string.IsNullOrWhiteSpace(config.OllamaModel.Value))
                        throw new ArgumentException("Ollama model is empty");
                    return new OllamaProvider(config.OllamaEndpoint.Value, config.OllamaModel.Value);

                case LLMProviderType.OpenAICompatible:
                    if (string.IsNullOrWhiteSpace(config.OpenAIEndpoint.Value))
                        throw new ArgumentException("OpenAI endpoint is empty");
                    if (string.IsNullOrWhiteSpace(config.OpenAIKey.Value))
                        throw new ArgumentException("OpenAI API key is missing");
                    if (string.IsNullOrWhiteSpace(config.OpenAIModel.Value))
                        throw new ArgumentException("OpenAI model is empty");
                    return new OpenAICompatibleProvider(config.OpenAIEndpoint.Value, config.OpenAIKey.Value, config.OpenAIModel.Value);

                case LLMProviderType.Kobold:
                    if (string.IsNullOrWhiteSpace(config.KoboldEndpoint.Value))
                        throw new ArgumentException("Kobold endpoint is empty");
                    return new KoboldProvider(config.KoboldEndpoint.Value, config.KoboldModel.Value);

                default:
                    throw new ArgumentOutOfRangeException(nameof(config.ProviderType), "Unsupported LLM provider type.");
            }
        }
    }
}
