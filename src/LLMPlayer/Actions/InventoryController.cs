using HarmonyLib;
using Assets.Scripts.Objects.Entities;

namespace LLMPlayer.Actions
{
    public class InventoryController
    {
        private readonly Human _human;

        public InventoryController(Human human)
        {
            _human = human;
        }

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
