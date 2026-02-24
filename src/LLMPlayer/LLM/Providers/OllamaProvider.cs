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
        private readonly string _endpoint;
        private readonly string _model;

        public OllamaProvider(string endpoint, string model)
        {
            _endpoint = endpoint;
            _client = new OllamaApiClient(endpoint);
            _model = model;
        }

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

        public bool ValidateConfig(out string error)
        {
            if (string.IsNullOrWhiteSpace(_endpoint) || !Uri.TryCreate(_endpoint, UriKind.Absolute, out _))
            {
                error = "Ollama endpoint is invalid or empty.";
                return false;
            }
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
