using HarmonyLib;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;
using LLMPlayer.Agent;

namespace LLMPlayer.Patches
{
    [HarmonyPatch(typeof(InGameMenu))]
    public class UIPatches
    {
        /// <summary>
        /// Injects a "SPAWN LLM BOT" button into the provided in-game menu by cloning the existing Respawn button and wiring its click handler to spawn an LLM bot.
        /// </summary>
        /// <param name="__instance">The InGameMenu instance being patched.</param>
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void AwakePostfix(InGameMenu __instance)
        {
            // Find the vertical layout group or button panel
            var panel = __instance.transform.Find("Panel");
            if (panel == null) return;

            // Create a new button
            // We can clone the Respawn button if it exists
            var respawnBtn = panel.Find("ButtonRespawn");
            if (respawnBtn != null)
            {
                var newBtn = Object.Instantiate(respawnBtn, panel);
                newBtn.name = "ButtonSpawnLLMBot";
                var text = newBtn.GetComponentInChildren<Text>();
                if (text != null) text.text = "SPAWN LLM BOT";

                var button = newBtn.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    NPCManager.Instance.SpawnBot();
                });
            }
        }
    }
}
