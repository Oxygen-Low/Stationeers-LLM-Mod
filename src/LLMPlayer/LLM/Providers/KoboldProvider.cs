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

        public async Task<string> GetResponseAsync(byte[] imageBytes, string systemPrompt, string userMessage, System.Threading.CancellationToken cancellationToken)
        {
            using (var cts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                cts.CancelAfter(TimeSpan.FromSeconds(30));
                try
                {
                    // Kobold.cpp standard multimodal format
                    var payload = new
                    {
                        prompt = $"{systemPrompt}\n\nUser: {userMessage}\nAssistant:",
                        images = (imageBytes != null && imageBytes.Length > 0) ? new[] { Convert.ToBase64String(imageBytes) } : new string[0],
                        max_context_length = 2048,
                        max_length = 512,
                        quiet = true,
                        model = _model
                    };

                    if (imageBytes == null || imageBytes.Length == 0)
                    {
                        Plugin.Instance.Log.LogWarning("Kobold Provider: Image bytes are null or empty. Sending text-only request.");
                    }

                    var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync($"{_endpoint}/api/v1/generate", content, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<KoboldResponse>(responseBody);
                        if (result != null && result.results != null && result.results.Count > 0 && !string.IsNullOrWhiteSpace(result.results[0].text))
                        {
                            return result.results[0].text;
                        }

                        Plugin.Instance.Log.LogWarning($"Kobold Provider: Received empty or missing text. Status: {response.StatusCode}, Body: {responseBody}");
                        return "Error: Empty or missing text in Kobold response";
                    }
                    else
                    {
                        return $"Error: {response.StatusCode}";
                    }
                }
                catch (System.OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    return "Error: Kobold request timed out.";
                }
                catch (Exception ex)
                {
                    Plugin.Instance.Log.LogError($"Kobold Error: {ex.Message}");
                    return $"Error: {ex.Message}";
                }
            }
        }

        public async Task<bool> CheckHealthAsync(System.Threading.CancellationToken cancellationToken)
        {
            using (var cts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                cts.CancelAfter(TimeSpan.FromSeconds(5));
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
            if (string.IsNullOrWhiteSpace(_endpoint) || !Uri.TryCreate(_endpoint, UriKind.Absolute, out _))
            {
                error = "Kobold endpoint is invalid or empty.";
                return false;
            }
            if (string.IsNullOrEmpty(_model))
            {
                error = "Kobold model is not configured.";
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
