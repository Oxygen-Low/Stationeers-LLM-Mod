using HarmonyLib;
using Assets.Scripts.Objects.Entities;

namespace LLMPlayer.Actions
{
    public class InteractionController
    {
        private readonly Human _human;

        public InteractionController(Human human)
        {
            _human = human ?? throw new System.ArgumentNullException(nameof(human));
        }

        public void Interact()
        {
            if (_human == null) return;

            // In Stationeers, interaction is complex, but often involves calling Interact()
            // on the object being looked at, or calling a method on the Human.

            // For now, we simulate the 'Interact' input.
            // If the human is an NPC, we might need to find what they are looking at.

            // Search for Interact method
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
    }
}
