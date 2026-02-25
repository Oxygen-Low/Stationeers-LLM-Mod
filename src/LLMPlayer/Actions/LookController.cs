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
            _human = human ?? throw new System.ArgumentNullException(nameof(human));
        }

        public void RotateYaw(float degrees)
        {
            try
            {
                if (_human != null && _human.transform != null)
                {
                    _human.transform.Rotate(Vector3.up, degrees);
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Instance.Log.LogError($"Error in RotateYaw: {ex.Message}");
            }
        }

        public void RotatePitch(float degrees)
        {
            try
            {
                // Pitch usually affects the head or camera
                // In Stationeers, there is a Head rotation
                var headTraverse = Traverse.Create(_human).Property("Head");
                if (headTraverse.PropertyExists())
                {
                    var head = headTraverse.GetValue<Transform>();
                    if (head != null)
                    {
                        head.Rotate(Vector3.right, degrees);
                    }
                }
                else
                {
                    Plugin.Instance.Log.LogWarning($"Property 'Head' not found on Human {_human.ReferenceId}");
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Instance.Log.LogError($"Error in RotatePitch: {ex.Message}");
            }
        }
    }
}
