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
            _dispatcher = new ActionDispatcher(_human);
            _llmProvider = LLMProviderFactory.CreateProvider();

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

        private IEnumerator AgentLoop()
        {
            while (_isActive)
            {
                yield return new WaitForSeconds(1.0f / Plugin.Instance.AgentTickRate.Value);

                if (_human == null || _human.IsDead) continue;

                // 1. Observe
                byte[] screenshot = null;
                bool captureDone = false;
                _screenshotService.CaptureScreenshot((bytes) => {
                    screenshot = bytes;
                    captureDone = true;
                });

                yield return new WaitUntil(() => captureDone);

                var context = GameStateExtractor.Extract(_human);
                var prompt = ContextBuilder.BuildPrompt(context);

                // 2. Reason (Query LLM)
                var task = _llmProvider.GetResponseAsync(screenshot, PromptTemplates.SystemPrompt, prompt);
                yield return new WaitUntil(() => task.IsCompleted);

                if (task.IsFaulted)
                {
                    Plugin.Instance.Log.LogError($"LLM Error: {task.Exception}");
                    continue;
                }

                var response = task.Result;

                // 3. Parse & Act
                var decision = ResponseParser.Parse(response);
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
            _isActive = active;
            if (active) StartCoroutine(AgentLoop());
            else StopAllCoroutines();
        }
    }
}
