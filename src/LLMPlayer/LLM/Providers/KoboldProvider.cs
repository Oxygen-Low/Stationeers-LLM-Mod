using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LLMPlayer.LLM.Providers
{
    public class KoboldProvider : ILLMProvider
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _endpoint;
        private readonly string _model;

        public KoboldProvider(string endpoint, string model)
        {
            _endpoint = endpoint.TrimEnd('/');
            _model = model;
        }

        public async Task<string> GetResponseAsync(byte[] imageBytes, string systemPrompt, string userMessage)
        {
            try
            {
                // Kobold.cpp standard multimodal format
                var payload = new
                {
                    prompt = $"{systemPrompt}\n\nUser: {userMessage}\nAssistant:",
                    images = new[] { Convert.ToBase64String(imageBytes) },
                    max_context_length = 2048,
                    max_length = 512,
                    quiet = true
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_endpoint}/api/v1/generate", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<KoboldResponse>(responseBody);
                    return result.results[0].text;
                }
                else
                {
                    return $"Error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                Plugin.Instance.Log.LogError($"Kobold Error: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_endpoint}/api/v1/model");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateConfig(out string error)
        {
            if (string.IsNullOrEmpty(_endpoint))
            {
                error = "Kobold endpoint is not configured.";
                return false;
            }
            error = null;
            return true;
        }

        private class KoboldResponse
        {
            public List<KoboldResult> results { get; set; }
        }

        private class KoboldResult
        {
            public string text { get; set; }
        }
    }
}
