using HarmonyLib;
using Assets.Scripts.Objects.Entities;

namespace LLMPlayer.Actions
{
    public class InteractionController
    {
        private readonly Human _human;

        /// <summary>
        /// Creates a new InteractionController that targets the specified Human instance.
        /// </summary>
        /// <param name="human">The Human instance whose interactions will be invoked by this controller.</param>
        public InteractionController(Human human)
        {
            _human = human;
        }

        /// <summary>
        /// Simulates an interaction input for the associated Human by invoking its interaction behavior.
        /// </summary>
        /// <remarks>
        /// Triggers the Human's interaction logic (for example, interacting with the object the Human is looking at or performing NPC-specific interaction behavior).
        /// </remarks>
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
