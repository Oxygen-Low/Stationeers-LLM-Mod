using HarmonyLib;
using Assets.Scripts.Objects.Entities;

namespace LLMPlayer.Actions
{
    public class ConstructionController
    {
        private readonly Human _human;

        public ConstructionController(Human human)
        {
            _human = human ?? throw new System.ArgumentNullException(nameof(human));
        }

        public void Construct()
        {
            try
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
                var interactMethod = Traverse.Create(_human).Method("Interact");
                if (interactMethod.MethodExists())
                {
                    interactMethod.GetValue();
                }
                else
                {
                    Plugin.Instance.Log.LogWarning($"Method 'Interact' not found on Human {_human.ReferenceId}");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Instance.Log.LogError($"Error in Construct: {ex.Message}");
            }
        }
    }
}
