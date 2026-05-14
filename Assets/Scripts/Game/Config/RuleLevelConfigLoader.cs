using System.Collections.Generic;
using System.Linq;
using cfg;
using cfg.rule;
using UnityEngine;

public static class RuleLevelConfigLoader
{
    public static RuleLevelTables Load(int levelId)
    {
        Tables tables = LubanConfig.LoadTables();
        List<LevelRuleStep> steps = tables.TbLevelRuleStep.DataList.Where(x => x.LevelId == levelId).ToList();
        List<LevelRuleEffect> effects = tables.TbLevelRuleEffect.DataList.Where(x => x.LevelId == levelId).ToList();
        Debug.LogWarning($"[RuleLevel] Loaded rule tables: level={levelId}, steps={steps.Count}, effects={effects.Count}");
        return steps.Count == 0 && effects.Count == 0
            ? null
            : new RuleLevelTables(levelId, steps, effects);
    }
}
