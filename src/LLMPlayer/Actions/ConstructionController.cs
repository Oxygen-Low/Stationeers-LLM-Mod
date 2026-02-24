using HarmonyLib;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects;

namespace LLMPlayer.Actions
{
    public class ConstructionController
    {
        private readonly Human _human;

        public ConstructionController(Human human)
        {
            _human = human;
        }

        public void Construct()
        {
            // Construction in Stationeers involves using a tool or material on a frame.
            // This is usually triggered by the 'Interact' action while holding the right item.

            // Logic to identify required tool/material:
            // 1. Find what we are looking at.
            // 2. Check its build requirements.
            // 3. Select the tool from inventory.
            // 4. Interact.

            // For now, this is a placeholder that calls Interact,
            // as the LLM is expected to select the tool itself via SELECT_SLOT.
            Traverse.Create(_human).Method("Interact").GetValue();
        }
    }
}
