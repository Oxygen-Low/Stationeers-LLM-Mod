using HarmonyLib;
using Assets.Scripts.Objects.Entities;

namespace LLMPlayer.Actions
{
    public class InventoryController
    {
        private readonly Human _human;

        public InventoryController(Human human)
        {
            _human = human ?? throw new System.ArgumentNullException(nameof(human));
        }

        public void SelectSlot(int index)
        {
            if (_human == null || _human.Slots == null) return;

            // Stationeers uses ActiveHandIndex or similar
            if (index >= 0 && index < _human.Slots.Count)
            {
                var swapMethod = Traverse.Create(_human).Method("CmdSwapActiveHand", index);
                if (swapMethod.MethodExists())
                {
                    swapMethod.GetValue();
                }
                else
                {
                    Plugin.Instance.Log.LogWarning($"Method 'CmdSwapActiveHand' not found on Human {_human.ReferenceId} for index {index}");
                }
            }
        }
    }
}
