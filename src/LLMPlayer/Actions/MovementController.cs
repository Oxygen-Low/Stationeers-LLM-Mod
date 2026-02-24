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
            _human = human;
        }

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

        public void Jump()
        {
            var movement = Traverse.Create(_human).Field("MovementController").GetValue();
            if (movement == null) return;
            Traverse.Create(movement).Method("SetJump", true).GetValue();
        }
    }
}
