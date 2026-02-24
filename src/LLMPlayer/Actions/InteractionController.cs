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
            // In Stationeers, interaction is complex, but often involves calling Interact()
            // on the object being looked at, or calling a method on the Human.

            // For now, we simulate the 'Interact' input.
            // If the human is an NPC, we might need to find what they are looking at.

            // Search for Interact method
            Traverse.Create(_human).Method("Interact").GetValue();
        }
    }
}
