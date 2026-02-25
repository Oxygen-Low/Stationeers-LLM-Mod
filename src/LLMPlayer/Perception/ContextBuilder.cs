using System.Collections.Generic;
using System.Text;
using LLMPlayer.Core;
using System.Linq;

namespace LLMPlayer.Perception
{
    public class ContextBuilder
    {
        public static string BuildPrompt(GameContext context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));

            var sb = new StringBuilder();
            sb.AppendLine("Current Game State:");
            sb.AppendLine($"- Position: {context.Position}");
            sb.AppendLine($"- Facing: {context.FacingDirection}");

            float health = context.Health;
            // Assume < 1.0 means normalized 0-1 range.
            // 1.0 is ambiguous but usually means 100% in normalized context.
            if (health < 1.0f)
            {
                health *= 100f;
            }
            sb.AppendLine($"- Health: {health:F1}% (Normalized)");

            sb.AppendLine($"- Held Item: {context.HeldItem ?? "None"}");

            sb.AppendLine("\nInventory:");
            var inventory = context.Inventory ?? new List<string>();
            foreach (var item in inventory)
            {
                sb.AppendLine($"  - {item}");
            }

            sb.AppendLine("\nNearby Objects:");
            var nearby = context.NearbyObjects ?? new List<InteractableObject>();
            foreach (var obj in nearby.OrderBy(o => o.Distance).Take(10))
            {
                sb.AppendLine($"  - {obj.Name} at {obj.Distance:F1}m (State: {obj.BuildState})");
            }

            return sb.ToString();
        }
    }
}
