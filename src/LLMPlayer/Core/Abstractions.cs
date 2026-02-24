using System.Collections.Generic;

namespace LLMPlayer.Core
{
    public interface IAgentAction
    {
        string Name { get; }
        Dictionary<string, string> Parameters { get; }
    }

    public class GameContext
    {
        public UnityEngine.Vector3 Position { get; set; }
        public UnityEngine.Vector3 Rotation { get; set; }
        public string FacingDirection { get; set; }
        public string HeldItem { get; set; }
        public List<string> Inventory { get; set; } = new List<string>();
        public List<InteractableObject> NearbyObjects { get; set; } = new List<InteractableObject>();
        public float Health { get; set; }
        public string SuitStatus { get; set; }
    }

    public class InteractableObject
    {
        public string Name { get; set; }
        public float Distance { get; set; }
        public string BuildState { get; set; }
        public string Description { get; set; }
    }

    public class AgentDecision
    {
        public string Reasoning { get; set; }
        public List<AgentAction> Actions { get; set; } = new List<AgentAction>();
    }

    public class AgentAction : IAgentAction
    {
        public string Name { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}
