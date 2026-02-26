using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using LLMPlayer.Agent;
using LLMPlayer.LLM;

namespace LLMPlayer
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "com.jules.stationeers.llmplayer";
        public const string NAME = "LLM Player";
        public const string VERSION = "1.0.0";

        public static Plugin Instance { get; private set; }
        public BepInEx.Logging.ManualLogSource Log => Logger;
        private Harmony _harmony;

        // Config Entries
        public ConfigEntry<LLMProviderType> ProviderType { get; private set; }
        public ConfigEntry<string> OllamaEndpoint { get; private set; }
        public ConfigEntry<string> OllamaModel { get; private set; }
        public ConfigEntry<string> OpenAIEndpoint { get; private set; }
        public string OpenAIKey { get; private set; }
        public ConfigEntry<string> OpenAIModel { get; private set; }
        public ConfigEntry<string> KoboldEndpoint { get; private set; }
        public ConfigEntry<string> KoboldModel { get; private set; }

        public ConfigEntry<float> AgentTickRate { get; private set; }
        public ConfigEntry<bool> DebugLogging { get; private set; }
        public ConfigEntry<int> ScreenshotResolution { get; private set; }
        public ConfigEntry<KeyCode> ToggleAiKey { get; private set; }

        private bool _aiActive = true;

        private void Awake()
        {
            Instance = this;
            _harmony = new Harmony(GUID);
            _harmony.PatchAll();

            SetupConfig();

            Logger.LogInfo($"{NAME} {VERSION} loaded!");

            // Initialize NPC Manager and other systems
            if (LLMPlayer.Agent.NPCManager.Instance == null)
            {
                gameObject.AddComponent<NPCManager>();
            }
        }

        private void Update()
        {
            if (ToggleAiKey.Value != KeyCode.None && Input.GetKeyDown(ToggleAiKey.Value))
            {
                _aiActive = !_aiActive;
                Log.LogInfo($"AI Control {(_aiActive ? "Enabled" : "Disabled")}");
                if (NPCManager.Instance != null)
                {
                    NPCManager.Instance.ToggleAllBots(_aiActive);
                }
                else
                {
                    Log.LogWarning("NPCManager instance missing, cannot toggle bots.");
                }
            }
        }

        private void SetupConfig()
        {
            ProviderType = Config.Bind("LLM Settings", "ProviderType", LLMProviderType.Ollama, "The LLM provider to use.");

            OllamaEndpoint = Config.Bind("Ollama", "Endpoint", "http://localhost:11434", "Ollama API endpoint.");
            OllamaModel = Config.Bind("Ollama", "Model", "gemma3:4b", "Ollama model name.");

            OpenAIEndpoint = Config.Bind("OpenAI", "Endpoint", "https://openrouter.ai/api/v1", "OpenAI-compatible API endpoint.");

            // OpenAIKey is loaded from environment variable or local file for security
            OpenAIKey = System.Environment.GetEnvironmentVariable("STATIONEERS_LLM_OPENAI_KEY")?.Trim();
            if (string.IsNullOrWhiteSpace(OpenAIKey))
            {
                try
                {
                    string keyPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "llm_openai_key.txt");
                    if (System.IO.File.Exists(keyPath))
                    {
                        string val = System.IO.File.ReadAllText(keyPath).Trim();
                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            OpenAIKey = val;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Log.LogError($"Failed to read OpenAI API key from file: {ex.Message}");
                }
            }

            OpenAIModel = Config.Bind("OpenAI", "Model", "google/gemma-2-9b-it:free", "Model name.");

            KoboldEndpoint = Config.Bind("Kobold", "Endpoint", "http://localhost:5001", "Kobold.cpp API endpoint.");
            KoboldModel = Config.Bind("Kobold", "Model", "", "Kobold.cpp model name (required).");

            AgentTickRate = Config.Bind("Agent", "TickRate", 1.0f,
                new ConfigDescription("How many times per second the agent should make a decision.", new AcceptableValueRange<float>(0.1f, 20.0f)));
            DebugLogging = Config.Bind("Debug", "VerboseLogging", false, "Enable detailed debug logging.");
            ScreenshotResolution = Config.Bind("Perception", "ResolutionScale", 512,
                new ConfigDescription("The square resolution of screenshots sent to LLM.", new AcceptableValueRange<int>(128, 1024)));
            ToggleAiKey = Config.Bind("Agent", "ToggleKey", KeyCode.F9, "Hotkey to toggle AI control for all bots.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            Instance = null;
            Logger.LogInfo($"{NAME} unloaded.");
        }
    }

    public enum LLMProviderType
    {
        Ollama,
        OpenAICompatible,
        Kobold
    }
}
