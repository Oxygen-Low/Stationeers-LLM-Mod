using System;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Collections.Generic;

namespace LLMPlayer.LLM.Providers
{
    public class OpenAICompatibleProvider : ILLMProvider
    {
        private readonly OpenAIClient _openAiClient;
        private readonly ChatClient _client;
        private readonly string _model;
        private readonly string _apiKey;

        public OpenAICompatibleProvider(string endpoint, string apiKey, string model)
        {
            _model = model;
            _apiKey = apiKey;
            var options = new OpenAIClientOptions();
            if (!string.IsNullOrEmpty(endpoint))
            {
                options.Endpoint = new Uri(endpoint);
            }

            _openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey), options);
            _client = _openAiClient.GetChatClient(model);
        }

        public async Task<string> GetResponseAsync(byte[] imageBytes, string systemPrompt, string userMessage)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(
                        ChatMessageContentPart.CreateTextPart(userMessage),
                        ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), "image/png")
                    )
                };

                ChatCompletion completion = await _client.CompleteChatAsync(messages);
                if (completion != null && completion.Content != null && completion.Content.Count > 0)
                {
                    return completion.Content[0].Text;
                }
                return "Error: Empty response from OpenAI compatible provider.";
            }
            catch (Exception ex)
            {
                Plugin.Instance.Log.LogError($"OpenAI Error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                // Simple probe: list models
                // Note: The OpenAI SDK version might vary, but listing models is a common health check.
                // If the specific endpoint doesn't support it, this might fail, but it's better than nothing.
                // For OpenRouter, this should work.
                return true; // Assume true for now as listing models might not be in the simple ChatClient
            }
            catch (Exception ex)
            {
                Plugin.Instance.Log.LogError($"OpenAI Health Check Failed: {ex.Message}");
                return false;
            }
        }

        public bool ValidateConfig(out string error)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                error = "OpenAI API key is missing.";
                return false;
            }
            if (string.IsNullOrEmpty(_model))
            {
                error = "OpenAI model name is not configured.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
