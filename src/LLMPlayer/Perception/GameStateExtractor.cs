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
        /// <summary>
        /// Builds a GameContext representing the provided human's current state.
        /// </summary>
        /// <param name="human">The human whose visible and measurable state should be captured. If null, an empty GameContext is returned.</param>
        /// <returns>A GameContext populated with the human's Position, Rotation, FacingDirection, Inventory entries (slot key and occupant or "Empty"), HeldItem (if any), NearbyObjects (each with Name, Distance, and BuildState), and Health.</returns>
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
            var activeSlot = Traverse.Create(human).Property("ActiveHandSlot").Method("Get").GetValue<Slot>();
            if (activeSlot != null)
            {
                var occupant = activeSlot.Get();
                if (occupant != null)
                {
                    context.HeldItem = occupant.DisplayName;
                }
            }

            // Nearby Objects (Simple sphere cast or distance check)
            var colliders = Physics.OverlapSphere(human.Position, 10f);
            foreach (var col in colliders)
            {
                var thing = col.GetComponentInParent<Thing>();
                if (thing != null && thing != human)
                {
                    context.NearbyObjects.Add(new InteractableObject
                    {
                        Name = thing.DisplayName,
                        Distance = Vector3.Distance(human.Position, thing.Position),
                        BuildState = GetBuildState(thing)
                    });
                }
            }

            context.Health = Traverse.Create(human).Field("Health").GetValue<float>();
            // context.SuitStatus = human.SuitStatus; // Need to find real field

            return context;
        }

        /// <summary>
        /// Determine the construction/build state of the given Thing.
        /// </summary>
        /// <param name="thing">The Thing whose build state should be reported.</param>
        /// <returns>A short string describing the Thing's build or construction state (for example: "Unknown", "Built", "UnderConstruction").</returns>
        private static string GetBuildState(Thing thing)
        {
            // Stationeers things often have a CurrentBuildStep
            // This is a guess, might need refinement
            return "Unknown";
        }
    }
}
