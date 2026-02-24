using System.Text;
using LLMPlayer.Core;
using System.Linq;

namespace LLMPlayer.Perception
{
    public class ContextBuilder
    {
        /// <summary>
        /// Builds a human-readable prompt describing the provided game's current state, inventory, and nearby objects.
        /// </summary>
        /// <param name="context">The game context containing position, facing direction, health, held item, inventory and nearby objects to include in the prompt.</param>
        /// <returns>A formatted multiline string with sections "Current Game State:", "Inventory:", and "Nearby Objects:"; nearby objects are ordered by distance and limited to the nearest 10, each shown as "Name at X.Xm (State: BuildState)".</returns>
        public static string BuildPrompt(GameContext context)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Current Game State:");
            sb.AppendLine($"- Position: {context.Position}");
            sb.AppendLine($"- Facing: {context.FacingDirection}");
            sb.AppendLine($"- Health: {context.Health}%");
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
