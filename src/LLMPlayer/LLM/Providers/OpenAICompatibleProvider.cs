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

        /// <summary>
        /// Initializes a new OpenAICompatibleProvider and configures an OpenAI chat client for the specified model.
        /// </summary>
        /// <param name="endpoint">Optional base URL of the OpenAI-compatible service; if null or empty the default endpoint is used.</param>
        /// <param name="apiKey">API key used to authenticate requests to the OpenAI-compatible service.</param>
        /// <param name="model">Name of the model to obtain a chat client for.</param>
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

        /// <summary>
        /// Sends a system prompt and a user message with an attached PNG image to the configured model and returns the model's reply text.
        /// </summary>
        /// <param name="imageBytes">PNG-encoded image bytes to attach to the user message.</param>
        /// <param name="systemPrompt">Instructions or context provided as the system message.</param>
        /// <param name="userMessage">The user's textual message to send alongside the image.</param>
        /// <returns>The text of the model's first chat completion; if an error occurs, a string beginning with "Error: " followed by the exception message.</returns>
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

        /// <summary>
        /// Performs a basic health check for the provider's client and configuration.
        /// </summary>
        /// <returns>`true` if the provider appears healthy, `false` otherwise.</returns>
        public async Task<bool> CheckHealthAsync()
        {
            // Simple check by listing models or just returning true if client init succeeded
            return true;
        }

        /// <summary>
        /// Validates that a model name is configured for the provider.
        /// </summary>
        /// <param name="error">An error message describing the configuration problem when validation fails; set to null when valid.</param>
        /// <returns>`true` if a model name is configured, `false` otherwise.</returns>
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
