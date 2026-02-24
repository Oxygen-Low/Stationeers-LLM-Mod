using System.Text;
using LLMPlayer.Core;
using System.Linq;

namespace LLMPlayer.Perception
{
    public class ContextBuilder
    {
        public static string BuildPrompt(GameContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Current Game State:");
            sb.AppendLine($"- Position: {context.Position}");
            sb.AppendLine($"- Facing: {context.FacingDirection}");

            float health = context.Health;
            if (health <= 1.0f) health *= 100f; // Scale 0-1 to 0-100
            sb.AppendLine($"- Health: {health:F1}%");

            sb.AppendLine($"- Held Item: {context.HeldItem ?? "None"}");

            sb.AppendLine("\nInventory:");
            foreach (var item in context.Inventory)
            {
                sb.AppendLine($"  - {item}");
            }

            sb.AppendLine("\nNearby Objects:");
            foreach (var obj in context.NearbyObjects.OrderBy(o => o.Distance).Take(10))
            {
                sb.AppendLine($"  - {obj.Name} at {obj.Distance:F1}m (State: {obj.BuildState})");
            }

            return sb.ToString();
        }
    }
}
