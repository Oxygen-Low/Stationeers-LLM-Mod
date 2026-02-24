using HarmonyLib;
using Assets.Scripts.Objects.Entities;
using UnityEngine;

namespace LLMPlayer.Actions
{
    public class MovementController
    {
        private readonly Human _human;

        /// <summary>
        /// Creates a MovementController tied to the specified human entity.
        /// </summary>
        /// <param name="human">The Human instance whose movement will be controlled.</param>
        public MovementController(Human human)
        {
            _human = human;
        }

        /// <summary>
        /// Applies a single cardinal movement input to the associated human's movement controller.
        /// </summary>
        /// <param name="direction">The movement direction to apply. Accepts "FORWARD", "BACKWARD", "LEFT", or "RIGHT" (case-insensitive); this sets the underlying movement input vector accordingly.</param>
        public void Move(string direction)
        {
            // For a bot, we might need to set the input values on the human's movement controller
            // or call a move method.
            var movement = Traverse.Create(_human).Field("MovementController").GetValue();
            if (movement == null) return;

            Vector2 moveInput = Vector2.zero;
            switch (direction.ToUpper())
            {
                case "FORWARD": moveInput.y = 1; break;
                case "BACKWARD": moveInput.y = -1; break;
                case "LEFT": moveInput.x = -1; break;
                case "RIGHT": moveInput.x = 1; break;
            }

            // In Stationeers, MovementController often has fields like MoveInput
            Traverse.Create(movement).Field("MoveInput").SetValue(moveInput);
        }

        /// <summary>
        /// Initiates a jump on the associated human's movement controller.
        /// </summary>
        /// <remarks>
        /// If the underlying movement controller is not available, the method does nothing.
        /// </remarks>
        public void Jump()
        {
            var movement = Traverse.Create(_human).Field("MovementController").GetValue();
            if (movement == null) return;
            Traverse.Create(movement).Method("SetJump", true).GetValue();
        }
    }
}
