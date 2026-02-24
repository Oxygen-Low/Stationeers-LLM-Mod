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
            using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30)))
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
                    var response = await _httpClient.PostAsync($"{_endpoint}/api/v1/generate", content, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<KoboldResponse>(responseBody);
                        if (result != null && result.results != null && result.results.Count > 0)
                        {
                            return result.results[0].text;
                        }
                        return "Error: Empty response from Kobold provider.";
                    }
                    else
                    {
                        return $"Error: {response.StatusCode}";
                    }
                }
                catch (System.OperationCanceledException)
                {
                    return "Error: Kobold request timed out.";
                }
                catch (Exception ex)
                {
                    Plugin.Instance.Log.LogError($"Kobold Error: {ex.Message}");
                    return $"Error: {ex.Message}";
                }
            }
        }

        public async Task<bool> CheckHealthAsync()
        {
            using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{_endpoint}/api/v1/model", cts.Token);
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
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
