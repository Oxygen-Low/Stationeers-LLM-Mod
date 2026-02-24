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
