using System.Collections.Generic;
using System.Linq;
using cfg.rule;

public sealed class RuleLevelTables
{
    public int LevelId { get; }
    public List<LevelRuleStep> Steps { get; }
    public List<LevelRuleEffect> Effects { get; }

    private readonly Dictionary<string, LevelRuleStep> _stepMap;
    private readonly Dictionary<string, LevelRuleEffect> _effectMap;

    public RuleLevelTables(
        int levelId,
        IEnumerable<LevelRuleStep> steps,
        IEnumerable<LevelRuleEffect> effects)
    {
        LevelId = levelId;
        Steps = steps.OrderBy(x => x.OrderIndex).ThenBy(x => x.Id).ToList();
        Effects = effects.OrderBy(x => x.Id).ToList();

        _stepMap = Steps.ToDictionary(x => x.Id, x => x);
        _effectMap = Effects
            .Where(x => !string.IsNullOrEmpty(x.Id))
            .ToDictionary(x => x.Id.Trim(), x => x);
    }

    public bool TryGetStep(string id, out LevelRuleStep step) => _stepMap.TryGetValue(id, out step);
    public bool TryGetEffect(string id, out LevelRuleEffect effect) => _effectMap.TryGetValue(id.Trim(), out effect);
}
