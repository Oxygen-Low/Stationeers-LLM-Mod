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

        /// <summary>
        /// Assigns this object to the static Instance property to initialize the NPCManager singleton.
        /// </summary>
        /// <remarks>Called by Unity when the script instance is being loaded.</remarks>
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Creates a new NPC bot based on the local human player, places it near the local player, attaches an AgentController, and registers it with the manager.
        /// </summary>
        /// <remarks>
        /// If the local human cannot be located, the method logs an error and returns without spawning.
        /// This method also logs progress and success messages to the plugin logger.
        /// </remarks>
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

            // Remove local player specific components if any
            // ...

            var controller = botObj.AddComponent<AgentController>();
            _bots.Add(controller);

            Plugin.Instance.Log.LogInfo("LLM Bot spawned successfully.");
        }

        /// <summary>
        /// Enables or disables all managed NPC bots.
        /// </summary>
        /// <param name="active">True to activate each bot, false to deactivate each bot.</param>
        public void ToggleAllBots(bool active)
        {
            foreach (var bot in _bots)
            {
                bot.SetActive(active);
            }
        }
    }
}
