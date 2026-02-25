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

        public async Task<string> GetResponseAsync(byte[] imageBytes, string systemPrompt, string userMessage, System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                var contentParts = new List<ChatMessageContentPart>();
                contentParts.Add(ChatMessageContentPart.CreateTextPart(userMessage));

                if (imageBytes != null && imageBytes.Length > 0)
                {
                    contentParts.Add(ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), "image/png"));
                }
                else
                {
                    Plugin.Instance.Log.LogWarning("OpenAI Provider: Image bytes are null or empty. Sending text-only request.");
                }

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(contentParts)
                };

                ChatCompletion completion = await _client.CompleteChatAsync(messages, null, cancellationToken);
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

        public async Task<bool> CheckHealthAsync(System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                // Simple probe: send a very small completion request with minimal tokens
                // or just listing models if we had the right client.
                // With ChatClient, we can try a very small completion.
                var messages = new List<ChatMessage> { new UserChatMessage("health check") };
                var options = new ChatCompletionOptions { MaxOutputTokenCount = 1 };
                await _client.CompleteChatAsync(messages, options, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Instance.Log.LogError($"OpenAI Health Check Failed: {ex.ToString()}");
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
