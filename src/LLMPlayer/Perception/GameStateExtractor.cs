using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using LLMPlayer.Core;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects;
using System.Linq;

namespace LLMPlayer.Perception
{
    public class GameStateExtractor
    {
        public static GameContext Extract(Human human)
        {
            var context = new GameContext();
            if (human == null) return context;

            context.Position = human.Position;
            context.Rotation = human.Rotation.eulerAngles;
            context.FacingDirection = human.transform.forward.ToString();

            // Extract Inventory
            foreach (var slot in human.Slots)
            {
                var occupant = slot.Get();
                if (occupant != null)
                {
                    context.Inventory.Add($"{slot.StringKey}: {occupant.DisplayName}");
                }
                else
                {
                    context.Inventory.Add($"{slot.StringKey}: Empty");
                }
            }

            // Held Item
            // Using reflection/AccessTools because these might be internal or obfuscated in some versions
            var activeHandTraverse = Traverse.Create(human).Property("ActiveHandSlot");
            if (activeHandTraverse.PropertyExists())
            {
                var activeSlot = activeHandTraverse.Method("Get").GetValue<Slot>();
                if (activeSlot != null)
                {
                    var occupant = activeSlot.Get();
                    if (occupant != null)
                    {
                        context.HeldItem = occupant.DisplayName;
                    }
                }
            }
            else
            {
                Plugin.Instance.Log.LogWarning($"Property 'ActiveHandSlot' not found on Human {human.ReferenceId}");
            }

            // Nearby Objects (Simple sphere cast or distance check)
            var colliders = Physics.OverlapSphere(human.Position, 10f);
            HashSet<Thing> seenThings = new HashSet<Thing>();
            foreach (var col in colliders)
            {
                var thing = col.GetComponentInParent<Thing>();
                if (thing != null && thing != human && !seenThings.Contains(thing))
                {
                    seenThings.Add(thing);
                    context.NearbyObjects.Add(new InteractableObject
                    {
                        Name = thing.DisplayName,
                        Distance = Vector3.Distance(human.Position, thing.Position),
                        BuildState = GetBuildState(thing)
                    });
                }
            }

            var healthTraverse = Traverse.Create(human).Field("Health");
            if (healthTraverse.FieldExists())
            {
                context.Health = healthTraverse.GetValue<float>();
            }
            else
            {
                Plugin.Instance.Log.LogWarning($"Field 'Health' not found on Human {human.ReferenceId}");
            }
            // context.SuitStatus = human.SuitStatus; // Need to find real field

            return context;
        }

        private static string GetBuildState(Thing thing)
        {
            // Stationeers things often have a CurrentBuildStep
            // This is a guess, might need refinement
            return "Unknown";
        }
    }
}
