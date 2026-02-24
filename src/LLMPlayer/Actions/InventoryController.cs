using HarmonyLib;
using Assets.Scripts.Objects.Entities;

namespace LLMPlayer.Actions
{
    public class InventoryController
    {
        private readonly Human _human;

        /// <summary>
        /// Initializes a new InventoryController for the specified human.
        /// </summary>
        /// <param name="human">The Human whose inventory (slots/active hand) this controller will operate on.</param>
        public InventoryController(Human human)
        {
            _human = human;
        }

        /// <summary>
        /// Sets the human's active inventory slot to the specified index when the index is within the valid range.
        /// </summary>
        /// <param name="index">Zero-based slot index to select; must be greater than or equal to 0 and less than the human's slot count. If out of range, no action is taken.</param>
        public void SelectSlot(int index)
        {
            // Stationeers uses ActiveHandIndex or similar
            if (index >= 0 && index < _human.Slots.Count)
            {
                Traverse.Create(_human).Method("CmdSwapActiveHand", index).GetValue();
            }
        }
    }
}
