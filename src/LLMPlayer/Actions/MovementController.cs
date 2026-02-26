using HarmonyLib;
using Assets.Scripts.Objects.Entities;
using UnityEngine;

namespace LLMPlayer.Actions
{
    public class MovementController
    {
        private readonly Human _human;

        public MovementController(Human human)
        {
            _human = human ?? throw new System.ArgumentNullException(nameof(human));
        }

        public void Move(string direction)
        {
            if (string.IsNullOrWhiteSpace(direction)) return;

            try
            {
                // For a bot, we might need to set the input values on the human's movement controller
                // or call a move method.
                var movement = Traverse.Create(_human).Field("MovementController").GetValue();
                if (movement == null)
                {
                    Plugin.Instance.Log.LogWarning("MovementController field not found on Human.");
                    return;
                }

                Vector2 moveInput = Vector2.zero;
                switch (direction.ToUpperInvariant())
                {
                    case "FORWARD": moveInput.y = 1; break;
                    case "BACKWARD": moveInput.y = -1; break;
                    case "LEFT": moveInput.x = -1; break;
                    case "RIGHT": moveInput.x = 1; break;
                }

                // In Stationeers, MovementController often has fields like MoveInput
                var moveInputTraverse = Traverse.Create(movement).Field("MoveInput");
                if (moveInputTraverse.FieldExists())
                {
                    moveInputTraverse.SetValue(moveInput);
                }
                else
                {
                    Plugin.Instance.Log.LogWarning("MoveInput field not found on MovementController.");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Instance.Log.LogError($"Error in Move: {ex.Message}");
            }
        }

        public void Jump()
        {
            try
            {
                var movement = Traverse.Create(_human).Field("MovementController").GetValue();
                if (movement == null) return;

                var setJumpTraverse = Traverse.Create(movement).Method("SetJump", true);
                if (setJumpTraverse.MethodExists())
                {
                    setJumpTraverse.GetValue();
                }
                else
                {
                    Plugin.Instance.Log.LogWarning("SetJump method not found on MovementController.");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Instance.Log.LogError($"Error in Jump: {ex.Message}");
            }
        }
    }
}
