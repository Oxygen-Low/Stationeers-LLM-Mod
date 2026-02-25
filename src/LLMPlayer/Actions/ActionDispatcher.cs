using System;
using System.Collections.Generic;
using System.Globalization;
using LLMPlayer.Core;
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

        public ActionDispatcher(Human human)
        {
            _movement = new MovementController(human);
            _look = new LookController(human);
            _inventory = new InventoryController(human);
            _interaction = new InteractionController(human);
            _construction = new ConstructionController(human);
        }

        public void Dispatch(AgentDecision decision)
        {
            if (decision == null || decision.Actions == null) return;

            foreach (var action in decision.Actions)
            {
                if (action != null)
                {
                    ExecuteAction(action);
                }
            }
        }

        private void ExecuteAction(AgentAction action)
        {
            if (action == null || string.IsNullOrWhiteSpace(action.Name))
            {
                Plugin.Instance.Log.LogWarning("Received null action or action with no name.");
                return;
            }

            string actionName = action.Name.ToUpperInvariant();
            Plugin.Instance.Log.LogInfo($"Executing Action: {actionName}");
            switch (actionName)
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

        private void HandleMove(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("direction", out var dir))
            {
                _movement.Move(dir);
            }
        }

        private void HandleLook(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("yaw", out var yawStr) &&
                float.TryParse(yawStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var yaw))
            {
                _look.RotateYaw(yaw);
            }
            if (parameters.TryGetValue("pitch", out var pitchStr) &&
                float.TryParse(pitchStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var pitch))
            {
                _look.RotatePitch(pitch);
            }
        }

        private void HandleSelectSlot(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("index", out var indexStr) && int.TryParse(indexStr, out var index))
            {
                _inventory.SelectSlot(index);
            }
        }
    }
}
