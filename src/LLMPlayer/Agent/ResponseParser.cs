using System;
using System.Collections.Generic;
using LLMPlayer.Core;
using System.Text.RegularExpressions;

namespace LLMPlayer.Agent
{
    public class ResponseParser
    {
        /// <summary>
        /// Parses a textual agent response into an AgentDecision containing extracted reasoning and actions.
        /// </summary>
        /// <param name="response">The response text containing an optional "Reasoning:" line and an optional "Actions:" section; lines are separated by CR/LF or LF.</param>
        /// <returns>An AgentDecision whose Reasoning is taken from the first line starting with "Reasoning:" and whose Actions are populated from subsequent lines beginning with "- NAME:".</returns>
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

                if (parsingActions && line.Trim().StartsWith("- NAME:"))
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

        /// <summary>
        /// Parses a single action line formatted as comma-separated `key:value` pairs into an AgentAction.
        /// </summary>
        /// <param name="line">A single action line (typically starting with '-' and containing entries like `NAME:Action, param:value`).</param>
        /// <returns>An AgentAction populated from the line's key/value pairs, or `null` if the line cannot be parsed.</returns>
        private static AgentAction ParseActionLine(string line)
        {
            try
            {
                var parts = line.Trim().TrimStart('-').Split(',');
                var action = new AgentAction();

                foreach (var part in parts)
                {
                    var kv = part.Split(':');
                    if (kv.Length == 2)
                    {
                        var key = kv[0].Trim().ToUpper();
                        var val = kv[1].Trim();

                        if (key == "NAME") action.Name = val;
                        else action.Parameters[key.ToLower()] = val;
                    }
                }
                return action;
            }
            catch
            {
                return null;
            }
        }
    }
}
