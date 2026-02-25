using UnityEngine;
using System.Collections;
using LLMPlayer.Perception;
using LLMPlayer.LLM;
using LLMPlayer.Actions;
using Assets.Scripts.Objects.Entities;

namespace LLMPlayer.Agent
{
    public class AgentController : MonoBehaviour
    {
        private Human _human;
        private ScreenshotService _screenshotService;
        private ActionDispatcher _dispatcher;
        private ILLMProvider _llmProvider;
        private bool _isActive = true;

        private void Awake()
        {
            _human = GetComponent<Human>();
            if (_human == null)
            {
                Plugin.Instance.Log.LogError($"Human component not found on GameObject {gameObject.name}. Agent initialization aborted.");
                _isActive = false;
                return;
            }

            _dispatcher = new ActionDispatcher(_human);

            try
            {
                _llmProvider = LLMProviderFactory.CreateProvider();
                if (!_llmProvider.ValidateConfig(out string error))
                {
                    Plugin.Instance.Log.LogError($"LLM Config Validation Failed: {error}");
                    _isActive = false;
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Instance.Log.LogError($"LLM Provider Initialization Failed: {ex.Message}");
                _isActive = false;
                return;
            }

            _screenshotService = gameObject.AddComponent<ScreenshotService>();

            // For bots, we need a camera.
            var camObj = new GameObject("AgentCamera");
            camObj.transform.SetParent(_human.transform);
            camObj.transform.localPosition = new Vector3(0, 1.7f, 0.2f); // Approx head position
            var camera = camObj.AddComponent<Camera>();
            camera.enabled = false; // We will render manually

            _screenshotService.Initialize(camera, Plugin.Instance.ScreenshotResolution.Value);

            StartCoroutine(AgentLoop());
        }

        private const float HEALTH_CHECK_TIMEOUT = 10.0f;

        private IEnumerator AgentLoop()
        {
            if (_llmProvider == null)
            {
                Plugin.Instance.Log.LogError("LLM Provider is null. Agent loop will not start.");
                _isActive = false;
                yield break;
            }

            // Health check before starting
            var healthTask = _llmProvider.CheckHealthAsync(System.Threading.CancellationToken.None);
            float healthTimer = 0;
            while (!healthTask.IsCompleted && healthTimer < HEALTH_CHECK_TIMEOUT)
            {
                healthTimer += Time.deltaTime;
                yield return null;
            }

            if (!healthTask.IsCompleted)
            {
                Plugin.Instance.Log.LogError("LLM Provider health check timed out. Agent loop will not start.");
                _isActive = false;
                yield break;
            }

            if (healthTask.IsFaulted || !healthTask.Result)
            {
                Plugin.Instance.Log.LogError("LLM Provider health check failed or returned unhealthy. Agent loop will not start.");
                _isActive = false;
                yield break;
            }

            while (_isActive)
            {
                float tickRate = Plugin.Instance.AgentTickRate.Value;
                if (tickRate <= 0.01f) tickRate = 0.01f; // Avoid division by zero
                yield return new WaitForSeconds(1.0f / tickRate);

                if (_human == null || _human.IsDead) continue;

                // 1. Observe
                byte[] screenshot = null;
                bool captureDone = false;
                _screenshotService.CaptureScreenshot((bytes) => {
                    screenshot = bytes;
                    captureDone = true;
                });

                float captureTimeout = 5.0f;
                float captureTimer = 0;
                while (!captureDone && captureTimer < captureTimeout)
                {
                    captureTimer += Time.deltaTime;
                    yield return null;
                }

                if (!captureDone)
                {
                    Plugin.Instance.Log.LogWarning("Screenshot capture timed out.");
                    continue;
                }

                var context = GameStateExtractor.Extract(_human);
                var prompt = ContextBuilder.BuildPrompt(context);

                // 2. Reason (Query LLM)
                var task = _llmProvider.GetResponseAsync(screenshot, PromptTemplates.SystemPrompt, prompt, System.Threading.CancellationToken.None);

                float llmTimeout = 30.0f;
                float llmTimer = 0;
                while (!task.IsCompleted && llmTimer < llmTimeout)
                {
                    llmTimer += Time.deltaTime;
                    yield return null;
                }

                if (!task.IsCompleted)
                {
                    Plugin.Instance.Log.LogWarning("LLM Request timed out.");
                    continue;
                }

                if (task.IsFaulted)
                {
                    Plugin.Instance.Log.LogError($"LLM Error: {task.Exception}");
                    continue;
                }

                var response = task.Result;

                // 3. Parse & Act
                var decision = ResponseParser.Parse(response);
                if (decision == null) continue;

                if (Plugin.Instance.DebugLogging.Value)
                {
                    Plugin.Instance.Log.LogInfo($"Agent Reasoning: {decision.Reasoning}");
                }

                _dispatcher.Dispatch(decision);
            }
        }

        public void SetActive(bool active)
        {
            if (_isActive == active) return;

            if (active)
            {
                if (_llmProvider == null)
                {
                    Plugin.Instance.Log.LogWarning("Cannot activate AI: LLM Provider is not initialized.");
                    return;
                }
                _isActive = true;
                StartCoroutine(AgentLoop());
            }
            else
            {
                _isActive = false;
                StopAllCoroutines();
            }
        }
    }
}
