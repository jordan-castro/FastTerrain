using System.Collections.Generic;
using Godot;

namespace FastTerrain;

public class AutoTiler
{
    private List<AutoTilerRule> Rules = null;
    private readonly Random Random = null;

    public AutoTiler(List<AutoTilerRule> rules, int seed)
    {
        Rules = rules;
        Random = new Random(seed);
    }

    public string DecideTile(Tile tile, DirectionMap<Vector2I> neighbours, GridSystem gridSystem)
    {
        // Get the rule for the tile
        AutoTilerRule rule = GetRuleForTile(tile);
        if (rule == null)
        {
            return tile.Name;
        }

        bool isMet = true;

        // Loop through conditions of the rule
        foreach (var condition in rule.Conditions)
        {
            isMet = true;

            // Check if the condition is met
            foreach (var direction in neighbours.Keys())
            {
                Vector2I value = neighbours[direction];
                if (value.X == -1 && value.Y == -1)
                {
                    continue;
                }

                if (!DefaultCondition(rule, condition[direction], value, gridSystem))
                {
                    isMet = false;
                    break;
                }
            }

            if (isMet)
            {
                if (condition.Result.Contains(","))
                {
                    return (string)Random.Choose(condition.Result.Split(','));
                }
                return condition.Result;
            }
        }

        return tile.Name;
    }

    private AutoTilerRule GetRuleForTile(Tile tile) {
        foreach (var rule in Rules)
        {
            if (rule.TileName == tile.Name)
            {
                return rule;
            }
        }
        return null;
    }

    private static bool AnyCondition() {
        return true;
    }

    private bool NotCondition(
        AutoTilerRule rule,
        string conditionValue,
        Vector2I neighbourValue,
        GridSystem gridSystem
    ) {
        string tileName = conditionValue.Split('!')[1];
        if (tileName == "Self") {
            return !SelfCondition(rule, neighbourValue, gridSystem);
        }

        return tileName != gridSystem.GetCell(neighbourValue).Name;
    }

    private bool SelfCondition(
        AutoTilerRule rule,
        Vector2I neighbourValue,
        GridSystem gridSystem
    ) {
        return rule.TileName == gridSystem.GetCell(neighbourValue).Name;
    }

    private bool AndCondition(
        AutoTilerRule rule,
        string conditionValue,
        Vector2I neighbourValue,
        GridSystem gridSystem
    ) {
        string[] conditions = conditionValue.Split('&');
        foreach (var condition in conditions)
        {
            if (!DefaultCondition(rule, condition, neighbourValue, gridSystem))
            {
                return false;
            }
        }
        return true;
    }

    private bool OrCondition(
        AutoTilerRule rule,
        string conditionValue,
        Vector2I neighbourValue,
        GridSystem gridSystem
    ) {
        string[] conditions = conditionValue.Split('|');
        foreach (var condition in conditions)
        {
            if (DefaultCondition(rule, condition, neighbourValue, gridSystem))
            {
                return true;
            }
        }
        return false;
    }

    private bool HasCondition(
        string conditionValue,
        Vector2I neighbourValue,
        GridSystem gridSystem
    ) {
        string tileName = conditionValue.Split('?')[1];
        return gridSystem.GetCell(neighbourValue).Name.Contains(tileName);
    }

    private bool DefaultCondition(
        AutoTilerRule rule,
        string conditionValue,
        Vector2I neighbourValue,
        GridSystem gridSystem
    ) {
        // Check null
        if (gridSystem.GetCellSafe(neighbourValue) == null)
        {
            return false;
        }

        if (conditionValue == "Any") {
            return AnyCondition();
        } else if (conditionValue == "Self") {
            return SelfCondition(rule, neighbourValue, gridSystem);
        }

        if (conditionValue.Contains('!'))
        {
            return NotCondition(rule, conditionValue, neighbourValue, gridSystem);
        }
        else if (conditionValue.Contains('&'))
        {
            return AndCondition(rule, conditionValue, neighbourValue, gridSystem);
        }
        else if (conditionValue.Contains('|'))
        {
            return OrCondition(rule, conditionValue, neighbourValue, gridSystem);
        }
        else if (conditionValue.Contains('?'))
        {
            return HasCondition(conditionValue, neighbourValue, gridSystem);
        }

        return conditionValue == gridSystem.GetCell(neighbourValue).Name;
    }
}