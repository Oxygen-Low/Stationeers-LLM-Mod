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

            // Remove local player specific components if any
            // ...

            var controller = botObj.AddComponent<AgentController>();
            _bots.Add(controller);

            Plugin.Instance.Log.LogInfo("LLM Bot spawned successfully.");
        }

        public void ToggleAllBots(bool active)
        {
            foreach (var bot in _bots)
            {
                bot.SetActive(active);
            }
        }
    }
}
