using System;
using System.Collections.Generic;
using LLMPlayer.Core;
using LLMPlayer.Actions;
using Assets.Scripts.Objects.Entities;

namespace LLMPlayer.Actions
{
    public class ActionDispatcher
    {
        private readonly MovementController _movement;
        private readonly LookController _look;
        private readonly InventoryController _inventory;
        private readonly InteractionController _interaction;
        private readonly ConstructionController _construction;

        /// <summary>
        /// Initializes a new ActionDispatcher and creates controller instances (movement, look, inventory, interaction, construction) bound to the given human.
        /// </summary>
        /// <param name="human">The Human instance whose controllers will be created and used by this dispatcher.</param>
        public ActionDispatcher(Human human)
        {
            _movement = new MovementController(human);
            _look = new LookController(human);
            _inventory = new InventoryController(human);
            _interaction = new InteractionController(human);
            _construction = new ConstructionController(human);
        }

        /// <summary>
        /// Executes each action contained in the given agent decision in sequence.
        /// </summary>
        /// <param name="decision">The agent decision whose Actions collection will be dispatched and executed in order.</param>
        public void Dispatch(AgentDecision decision)
        {
            foreach (var action in decision.Actions)
            {
                ExecuteAction(action);
            }
        }

        /// <summary>
        /// Dispatches a single AgentAction to the appropriate controller based on its Name and executes it.
        /// </summary>
        /// <param name="action">The action to execute; its Name determines the operation and its Parameters supply handler-specific data (e.g., "direction" for MOVE, "yaw"/"pitch" for LOOK, "index" for SELECT_SLOT).</param>
        private void ExecuteAction(AgentAction action)
        {
            Plugin.Instance.Log.LogInfo($"Executing Action: {action.Name}");
            switch (action.Name.ToUpper())
            {
                case "MOVE":
                    HandleMove(action.Parameters);
                    break;
                case "LOOK":
                    HandleLook(action.Parameters);
                    break;
                case "SELECT_SLOT":
                    HandleSelectSlot(action.Parameters);
                    break;
                case "INTERACT":
                    _interaction.Interact();
                    break;
                case "CONSTRUCT":
                    _construction.Construct();
                    break;
                case "JUMP":
                    _movement.Jump();
                    break;
                default:
                    Plugin.Instance.Log.LogWarning($"Unknown action: {action.Name}");
                    break;
            }
        }

        /// <summary>
        /// Extracts a "direction" parameter from the provided parameters and instructs the movement controller to move in that direction.
        /// </summary>
        /// <param name="parameters">A dictionary of action parameters; if it contains a "direction" entry, its value is passed to the movement controller.</param>
        private void HandleMove(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("direction", out var dir))
            {
                _movement.Move(dir);
            }
        }

        /// <summary>
        /// Applies yaw and/or pitch rotations to the look controller using values from the provided parameters.
        /// </summary>
        /// <param name="parameters">A dictionary that may contain "yaw" and/or "pitch" string values; each value, if present and parsable as a float, is applied as the corresponding rotation.</param>
        private void HandleLook(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("yaw", out var yawStr) && float.TryParse(yawStr, out var yaw))
            {
                _look.RotateYaw(yaw);
            }
            if (parameters.TryGetValue("pitch", out var pitchStr) && float.TryParse(pitchStr, out var pitch))
            {
                _look.RotatePitch(pitch);
            }
        }

        /// <summary>
        /// Selects an inventory slot when the parameters contain a valid "index" value.
        /// </summary>
        /// <param name="parameters">A map of action parameters; if it contains an "index" key whose value can be parsed as an integer, the corresponding inventory slot will be selected. </param>
        private void HandleSelectSlot(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("index", out var indexStr) && int.TryParse(indexStr, out var index))
            {
                _inventory.SelectSlot(index);
            }
        }
    }
}
