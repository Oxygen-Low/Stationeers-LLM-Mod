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

        /// <summary>
        /// Initializes a new instance of KoboldProvider with the specified Kobold API endpoint and model name.
        /// </summary>
        /// <param name="endpoint">Base URL of the Kobold API (for example, "http://host:port").</param>
        /// <param name="model">Model name to target on the Kobold server.</param>
        public KoboldProvider(string endpoint, string model)
        {
            _endpoint = endpoint.TrimEnd('/');
            _model = model;
        }

        /// <summary>
        /// Sends a multimodal request (image plus prompts) to the configured Kobold endpoint and returns the assistant's generated text.
        /// </summary>
        /// <param name="imageBytes">Image data as a byte array; it will be Base64-encoded for the request.</param>
        /// <param name="systemPrompt">System-level prompt that guides the assistant's behavior.</param>
        /// <param name="userMessage">User-provided message to include in the prompt.</param>
        /// <returns>The assistant's generated text from the first result, or an error string prefixed with "Error: " if the request fails or an exception occurs.</returns>
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

        /// <summary>
        /// Checks whether the configured Kobold model endpoint is healthy.
        /// </summary>
        /// <returns>`true` if the endpoint responded with a successful HTTP status code, `false` otherwise (including network or request errors).</returns>
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

        /// <summary>
        /// Validates that a Kobold endpoint is configured for this provider.
        /// </summary>
        /// <param name="error">When validation fails, contains an explanatory error message; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the endpoint is configured, <c>false</c> otherwise.</returns>
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
