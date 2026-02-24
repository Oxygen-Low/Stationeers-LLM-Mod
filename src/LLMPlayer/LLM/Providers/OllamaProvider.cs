using System;
using System.Threading.Tasks;
using OllamaSharp;
using OllamaSharp.Models;
using System.Linq;

namespace LLMPlayer.LLM.Providers
{
    public class OllamaProvider : ILLMProvider
    {
        private readonly OllamaApiClient _client;
        private readonly string _model;

        /// <summary>
        /// Initializes a new instance of <see cref="OllamaProvider"/> that communicates with an Ollama API at the specified endpoint and uses the specified model.
        /// </summary>
        /// <param name="endpoint">Base URL or address of the Ollama API endpoint.</param>
        /// <param name="model">Name or identifier of the Ollama model to use for requests.</param>
        public OllamaProvider(string endpoint, string model)
        {
            _client = new OllamaApiClient(endpoint);
            _model = model;
        }

        /// <summary>
        /// Sends the provided image (base64-encoded) together with a system prompt and user message to the configured Ollama model and returns the model's combined response.
        /// </summary>
        /// <param name="imageBytes">Image data to include in the request; the bytes will be base64-encoded.</param>
        /// <param name="systemPrompt">System-level instruction or context to provide to the model.</param>
        /// <param name="userMessage">The user prompt or message to send to the model.</param>
        /// <returns>The concatenated model response text; on failure returns a string beginning with "Error: " followed by the error message.</returns>
        public async Task<string> GetResponseAsync(byte[] imageBytes, string systemPrompt, string userMessage)
        {
            try
            {
                var request = new GenerateRequest
                {
                    Model = _model,
                    Prompt = userMessage,
                    System = systemPrompt,
                    Images = new[] { Convert.ToBase64String(imageBytes) },
                    Stream = false
                };

                string fullResponse = "";
                await foreach (var response in _client.Generate(request))
                {
                    fullResponse += response.Response;
                }
                return fullResponse;
            }
            catch (Exception ex)
            {
                Plugin.Instance.Log.LogError($"Ollama Error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Checks whether the configured Ollama backend is reachable and currently running.
        /// </summary>
        /// <returns>`true` if the Ollama service is running and reachable; `false` otherwise (including when an error occurs).</returns>
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                return await _client.IsRunning();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that a model name is configured for the Ollama provider.
        /// </summary>
        /// <param name="error">When validation fails, contains a human-readable error message; otherwise null.</param>
        /// <returns>`true` if a model name is configured, `false` otherwise.</returns>
        public bool ValidateConfig(out string error)
        {
            if (string.IsNullOrEmpty(_model))
            {
                error = "Ollama model name is not configured.";
                return false;
            }
            error = null;
            return true;
        }
    }
}
