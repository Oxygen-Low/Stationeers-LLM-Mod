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
        public ConfigEntry<string> OpenAIKey { get; private set; }
        public ConfigEntry<string> OpenAIModel { get; private set; }
        public ConfigEntry<string> KoboldEndpoint { get; private set; }
        public ConfigEntry<string> KoboldModel { get; private set; }

        public ConfigEntry<float> AgentTickRate { get; private set; }
        public ConfigEntry<bool> DebugLogging { get; private set; }
        public ConfigEntry<int> ScreenshotResolution { get; private set; }
        public ConfigEntry<KeyCode> ToggleAiKey { get; private set; }

        private bool _aiActive = true;

        /// <summary>
        /// Initialize the plugin: set the singleton instance, apply Harmony patches, load configuration, log startup, and attach the NPCManager component.
        /// </summary>
        private void Awake()
        {
            Instance = this;
            _harmony = new Harmony(GUID);
            _harmony.PatchAll();

            SetupConfig();

            Logger.LogInfo($"{NAME} {VERSION} loaded!");

            // Initialize NPC Manager and other systems
            gameObject.AddComponent<NPCManager>();
        }

        /// <summary>
        /// Monitors the configured toggle key and toggles global AI control for all NPCs when the key is pressed.
        /// </summary>
        /// <remarks>
        /// The new AI state is logged and applied to all bots.
        private void Update()
        {
            if (ToggleAiKey.Value != KeyCode.None && Input.GetKeyDown(ToggleAiKey.Value))
            {
                _aiActive = !_aiActive;
                Log.LogInfo($"AI Control {(_aiActive ? "Enabled" : "Disabled")}");
                NPCManager.Instance.ToggleAllBots(_aiActive);
            }
        }

        /// <summary>
        /// Registers and binds all plugin configuration entries used at runtime.
        /// </summary>
        /// <remarks>
        /// Initializes configuration sections and keys for LLM provider selection, provider endpoints and models (Ollama, OpenAI-compatible, Kobold), agent behavior (tick rate, toggle key), debug logging, and perception screenshot resolution.
        /// </remarks>
        private void SetupConfig()
        {
            ProviderType = Config.Bind("LLM Settings", "ProviderType", LLMProviderType.Ollama, "The LLM provider to use.");

            OllamaEndpoint = Config.Bind("Ollama", "Endpoint", "http://localhost:11434", "Ollama API endpoint.");
            OllamaModel = Config.Bind("Ollama", "Model", "gemma3:4b", "Ollama model name.");

            OpenAIEndpoint = Config.Bind("OpenAI", "Endpoint", "https://openrouter.ai/api/v1", "OpenAI-compatible API endpoint.");
            OpenAIKey = Config.Bind("OpenAI", "ApiKey", "", "API key for the provider.");
            OpenAIModel = Config.Bind("OpenAI", "Model", "google/gemma-2-9b-it:free", "Model name.");

            KoboldEndpoint = Config.Bind("Kobold", "Endpoint", "http://localhost:5001", "Kobold.cpp API endpoint.");
            KoboldModel = Config.Bind("Kobold", "Model", "", "Kobold.cpp model name (optional for some setups).");

            AgentTickRate = Config.Bind("Agent", "TickRate", 1.0f, "How many times per second the agent should make a decision.");
            DebugLogging = Config.Bind("Debug", "VerboseLogging", true, "Enable detailed debug logging.");
            ScreenshotResolution = Config.Bind("Perception", "ResolutionScale", 512, "The square resolution of screenshots sent to LLM.");
            ToggleAiKey = Config.Bind("Agent", "ToggleKey", KeyCode.F9, "Hotkey to toggle AI control for all bots.");
        }

        /// <summary>
        /// Cleans up plugin state when the GameObject is destroyed.
        /// </summary>
        /// <remarks>
        /// Removes any Harmony patches applied by this plugin and logs an unload message.
        /// </remarks>
        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
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
