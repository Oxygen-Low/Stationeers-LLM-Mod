using HarmonyLib;
using Assets.Scripts.Objects.Entities;
using UnityEngine;

namespace LLMPlayer.Actions
{
    public class LookController
    {
        private readonly Human _human;

        public LookController(Human human)
        {
            _human = human;
        }

        public void RotateYaw(float degrees)
        {
            _human.transform.Rotate(Vector3.up, degrees);
        }

        public void RotatePitch(float degrees)
        {
            // Pitch usually affects the head or camera
            // In Stationeers, there is a Head rotation
            var head = Traverse.Create(_human).Property("Head").GetValue<Transform>();
            if (head != null)
            {
                head.Rotate(Vector3.right, degrees);
            }
        }
    }
}
