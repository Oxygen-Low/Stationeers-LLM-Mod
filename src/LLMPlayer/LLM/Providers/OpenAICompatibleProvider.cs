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
        private readonly ChatClient _client;
        private readonly string _model;

        public OpenAICompatibleProvider(string endpoint, string apiKey, string model)
        {
            _model = model;
            var options = new OpenAIClientOptions();
            if (!string.IsNullOrEmpty(endpoint))
            {
                options.Endpoint = new Uri(endpoint);
            }

            var client = new OpenAIClient(new ApiKeyCredential(apiKey), options);
            _client = client.GetChatClient(model);
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
                return completion.Content[0].Text;
            }
            catch (Exception ex)
            {
                Plugin.Instance.Log.LogError($"OpenAI Error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<bool> CheckHealthAsync()
        {
            // Simple check by listing models or just returning true if client init succeeded
            return true;
        }

        public bool ValidateConfig(out string error)
        {
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
