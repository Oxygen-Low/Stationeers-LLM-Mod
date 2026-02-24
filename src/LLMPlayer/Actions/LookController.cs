using HarmonyLib;
using Assets.Scripts.Objects.Entities;
using UnityEngine;

namespace LLMPlayer.Actions
{
    public class LookController
    {
        private readonly Human _human;

        /// <summary>
        /// Initializes a new LookController for the specified Human.
        /// </summary>
        /// <param name="human">The Human whose look (yaw and pitch) this controller will manipulate.</param>
        public LookController(Human human)
        {
            _human = human;
        }

        /// <summary>
        /// Rotates the associated Human around its vertical (yaw) axis by the given angle.
        /// </summary>
        /// <param name="degrees">Angle in degrees to rotate around the up (Y) axis; positive values rotate in the axis' positive direction.</param>
        public void RotateYaw(float degrees)
        {
            _human.transform.Rotate(Vector3.up, degrees);
        }

        /// <summary>
        /// Rotates the human's head around its local right (X) axis by the given number of degrees.
        /// </summary>
        /// <param name="degrees">Degrees to rotate around the head's local right (pitch) axis.</param>
        /// <remarks>If the human has no head transform, no rotation is performed.</remarks>
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
