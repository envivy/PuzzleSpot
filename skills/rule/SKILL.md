---
name: rule
description: Design, extend, and implement configuration-driven find-difference or hidden-object level rules in this PuzzleSpot Unity project. Use when building or modifying table-driven interactions such as click, drag-to-target, swipe, multi-step hints, prerequisite chains, fail states, score outcomes, element visibility changes, or authoring Luban rule tables and supporting runtime scripts.
---

# Rule Skill

Implement level interactions through Luban rule tables first, and only add bespoke code when a mechanic truly cannot fit the table model.

## Working Files

- Rule schemas: `DataTables/Defines/Rule.xml`
- Rule data: `DataTables/Datas/level-rule-*.xlsx`
- Runtime loader: `Assets/Scripts/Game/Config/RuleLevelConfigLoader.cs`
- Runtime executor: `Assets/Scripts/Game/GamePlay/Rule/`
- Generated Luban code: `Assets/Scripts/Gen/Luban/`

## Authoring Model

Represent one playable level with:

- `LevelRuleLevel`: level-wide metadata
- `LevelRuleHint`: scoring hints; one hint usually maps to one point
- `LevelRuleStep`: one player action or one action inside a multi-step hint
- `LevelRuleOutcome`: the result of a successful or failed step
- `LevelRuleEffect`: scene mutations such as show/hide/swap/enable/state changes

Prefer this pattern:

1. Put interaction prerequisites in `require_step1/2` and `state_requirement1/2`
2. Put score/fail semantics in `LevelRuleOutcome`
3. Put visual or object changes in `LevelRuleEffect`
4. Keep prefab wiring in `RuleElementRegistry`, keyed by stable string ids

## Interaction Mapping

- Click: `op_type=Click`, set `actor_id`
- Drag to target: `op_type=DragToTarget`, set `actor_id` and `target_id`
- Swipe: `op_type=Swipe`, set `actor_id` and `direction`
- Drag end trigger: `op_type=DragEnd`, set `actor_id`

## ID Naming Rule

Use fixed per-level ids to prevent duplicates across scenes and tables.

- Target object id: `L{levelId}_T{index}`
- Actor object id: `L{levelId}_A{index}`

Examples:

- first target in level 1: `L01_T01`
- second target in level 1: `L01_T02`
- first actor in level 1: `L01_A01`
- third actor in level 12: `L12_A03`

Rules:

1. `levelId` uses 2 digits for current content, such as `01`, `02`, `12`
2. `index` uses 2 digits, such as `01`, `02`, `03`
3. Keep ids ASCII only
4. Do not rename ids after they are wired into scene objects and tables unless all references are updated together
5. If an object needs a readable label, keep that in comments, object names, or extra table fields instead of bloating the id itself

## Dependency Patterns

- Require an earlier action:
  Put the earlier `step.id` in `require_step1` or `require_step2`
- Prevent a step after another path:
  Put the conflicting `step.id` in `block_step1` or `block_step2`
- Require a state like `box_open=true`:
  Add an effect `SetState` on the first step, then add `state_requirement1` or `state_requirement2` on later steps
- Multi-step hint:
  Give related steps the same `hint_id` and use `group_id` plus `order_index`
- Fail action:
  Set `fail_on_trigger=true` and connect `fail_outcome1/2`

## Effects Guidance

Prefer combining simple effects instead of inventing one huge effect.

- show object: `ShowElement`
- hide object: `HideElement`
- replace old with new: `SwapElement`
- unlock interaction after a prerequisite: `EnableElement`
- remember logical progress: `SetState`

If a new mechanic is needed, extend `Rule.xml`, the runtime enums, and `RuleLevelController` together.

## Workflow

1. Update `DataTables/Defines/Rule.xml` only when the mechanic model must expand
2. Edit the relevant `level-rule-*.xlsx` tables
3. Run `DataTables/gen_client.bat`
4. Let the script regenerate and patch Luban code imports
5. Verify the target prefab has matching `RuleElementRegistry`, `RuleOperation`, and `RuleTarget` ids

## Guardrails

- Do not hardcode level-specific scoring logic in `Level*.cs` if the rule tables can express it
- Keep ids stable and human-readable for scene wiring
- Follow the fixed id rule `Lxx_Txx` and `Lxx_Axx` for all new rule levels
- Add bespoke code only for presentation details or mechanics that tables genuinely cannot model yet
- When extending the table model, update this skill so later work follows the same rule language
