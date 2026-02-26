using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Objects.Entities;
using HarmonyLib;

namespace LLMPlayer.Agent
{
    public class NPCManager : MonoBehaviour
    {
        public static NPCManager Instance { get; private set; }
        private List<AgentController> _bots = new List<AgentController>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnBot()
        {
            Plugin.Instance.Log.LogInfo("Attempting to spawn LLM Bot...");

            // In Stationeers, we can find a spawn point or use local human position
            var localHuman = Traverse.Create(typeof(Human)).Property("LocalHuman").GetValue<Human>();
            if (localHuman == null)
            {
                Plugin.Instance.Log.LogError("Local human not found. Cannot spawn bot.");
                return;
            }

            // This is a simplification. Real spawning might need NetworkManager.
            // We try to instantiate a new Human.
            GameObject botObj = Instantiate(localHuman.gameObject, localHuman.Position + localHuman.transform.forward * 2f, localHuman.transform.rotation);
            var botHuman = botObj.GetComponent<Human>();
            if (botHuman == null)
            {
                Plugin.Instance.Log.LogError("Cloned GameObject does not have a Human component. Destroying bot.");
                Destroy(botObj);
                return;
            }

            // Sanitize bot: Remove/Disable local player specific components
            // This is a list of typical components that might cause issues if cloned
            string[] localComponents = { "LocalPlayerController", "InputHandler", "PlayerInput", "Camera", "AudioListener", "PlayerHUD" };
            foreach (var compName in localComponents)
            {
                var comp = botObj.GetComponent(compName);
                if (comp != null)
                {
                    if (comp is Behaviour b) b.enabled = false;
                    else Destroy(comp);
                }
            }

            // Also check children for cameras/listeners
            foreach (var cam in botObj.GetComponentsInChildren<Camera>()) cam.enabled = false;
            // Disable AudioListeners via reflection/name to avoid extra dependencies
            foreach (var listener in botObj.GetComponentsInChildren<Behaviour>())
            {
                if (listener.GetType().Name == "AudioListener") listener.enabled = false;
            }

            var controller = botObj.AddComponent<AgentController>();
            _bots.Add(controller);

            Plugin.Instance.Log.LogInfo("LLM Bot spawned and sanitized successfully.");
        }

        public void ToggleAllBots(bool active)
        {
            _bots.RemoveAll(b => b == null);
            foreach (var bot in _bots)
            {
                bot.SetActive(active);
            }
        }
    }
}
