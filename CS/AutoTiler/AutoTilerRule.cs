using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace FastTerrain;

public class AutoTilerRule {
    public string TileName = null;
    public List<AutoTilerRuleCondition> Conditions = null;

    public AutoTilerRule(string tileName, List<AutoTilerRuleCondition> conditions) {
        TileName = tileName;
        Conditions = conditions;
    }

    public static AutoTilerRule FromJson(JObject json) {
        string tileName = (string)json["tile"];
        List<AutoTilerRuleCondition> conditions = new();
        foreach (var condition in json["conditions"]) {
            conditions.Add(
                new AutoTilerRuleCondition(
                    (string)condition["result"],
                    (string)condition["N"],
                    (string)condition["E"],
                    (string)condition["S"],
                    (string)condition["W"]
                )
            );
        }

        return new AutoTilerRule(tileName, conditions);
    }
}