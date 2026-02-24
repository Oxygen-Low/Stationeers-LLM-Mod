using System;
using System.Collections.Generic;
using LLMPlayer.Core;

namespace LLMPlayer.Agent
{
    public class ResponseParser
    {
        public static AgentDecision Parse(string response)
        {
            var decision = new AgentDecision();
            var lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            bool parsingActions = false;
            foreach (var line in lines)
            {
                if (line.StartsWith("Reasoning:", StringComparison.OrdinalIgnoreCase))
                {
                    decision.Reasoning = line.Substring(line.IndexOf(':') + 1).Trim();
                    continue;
                }

                if (line.StartsWith("Actions:", StringComparison.OrdinalIgnoreCase))
                {
                    parsingActions = true;
                    continue;
                }

                if (parsingActions && line.Trim().StartsWith("- NAME:", StringComparison.OrdinalIgnoreCase))
                {
                    var action = ParseActionLine(line);
                    if (action != null)
                    {
                        decision.Actions.Add(action);
                    }
                }
            }
            return decision;
        }

        private static AgentAction ParseActionLine(string line)
        {
            try
            {
                var parts = line.Trim().TrimStart('-').Split(',');
                var action = new AgentAction();

                foreach (var part in parts)
                {
                    var kv = part.Split(new[] { ':' }, 2);
                    if (kv.Length == 2)
                    {
                        var key = kv[0].Trim().ToUpperInvariant();
                        var val = kv[1].Trim();

                        if (key == "NAME") action.Name = val;
                        else action.Parameters[key.ToLowerInvariant()] = val;
                    }
                }
                return action;
            }
            catch (Exception ex)
            {
                Plugin.Instance.Log.LogError($"Error parsing action line '{line}': {ex.Message}");
                return null;
            }
        }
    }
}
