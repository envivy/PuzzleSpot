using System.Collections.Generic;
using System.Linq;
using cfg.rule;
using UnityEngine;

public class RuleLevelController : Level
{
    private RuleElementRegistry _elementRegistry;

    private RuleLevelTables _tables;
    private readonly HashSet<string> _completedSteps = new HashSet<string>();
    private readonly HashSet<string> _completedHints = new HashSet<string>();
    private readonly Dictionary<string, bool> _stateFlags = new Dictionary<string, bool>();
    private readonly Dictionary<string, List<LevelRuleStep>> _actorStepByElement = new Dictionary<string, List<LevelRuleStep>>();

    private void Awake()
    {
        _tables = RuleLevelConfigLoader.Load(levelID);
        _elementRegistry = GetComponent<RuleElementRegistry>();
        
        if (_elementRegistry != null)
        {
            _elementRegistry.Rebuild();
        }

        BuildCaches();
    }

    public override void RefreshUIState(int elementID)
    {
    }

    public bool TryHandleInteraction(string actorId, RuleInteractionType interactionType, string targetId = null, RuleOperation sourceOperation = null, RuleSwipeDirection direction = RuleSwipeDirection.Any)
    {
        if (!_actorStepByElement.TryGetValue(actorId, out List<LevelRuleStep> steps))
        {
            return false;
        }

        foreach (LevelRuleStep step in steps)
        {
            if (!IsStepAvailable(step, targetId))
            {
                continue;
            }

            ExecuteStep(step, sourceOperation);
            return true;
        }

        return false;
    }

    public string GetNextHintText()
    {
        string nextHint = _tables.Steps
            .Select(step => step.HintId)
            .FirstOrDefault(hintId => !string.IsNullOrEmpty(hintId) && !_completedHints.Contains(hintId));
        return nextHint ?? string.Empty;
    }

    private void BuildCaches()
    {
        if (_tables == null)
        {
            return;
        }

        foreach (LevelRuleStep step in _tables.Steps)
        {
            if (!_actorStepByElement.TryGetValue(step.ActorId, out List<LevelRuleStep> elementSteps))
            {
                elementSteps = new List<LevelRuleStep>();
                _actorStepByElement.Add(step.ActorId, elementSteps);
            }

            elementSteps.Add(step);
        }

        foreach (List<LevelRuleStep> value in _actorStepByElement.Values)
        {
            value.Sort((a, b) =>
            {
                int orderCompare = a.OrderIndex.CompareTo(b.OrderIndex);
                if (orderCompare != 0)
                {
                    return orderCompare;
                }

                return string.CompareOrdinal(a.Id, b.Id);
            });
        }
    }

    private bool IsStepAvailable(LevelRuleStep step, string targetId)
    {
        if (_completedSteps.Contains(step.Id))
        {
            Debug.LogWarning($"[RuleLevel] Step already completed: {step.Id}");
            return false;
        }

        if (!string.IsNullOrEmpty(step.TargetId) && step.TargetId != targetId)
        {
            Debug.LogWarning($"[RuleLevel] Step target mismatch: step={step.Id}, expected={step.TargetId}, actual={targetId}");
            return false;
        }

        foreach (string requireId in EnumerateStringSlots(step.RequireStep))
        {
            if (!string.IsNullOrEmpty(requireId) && !_completedSteps.Contains(requireId))
            {
                Debug.LogWarning($"[RuleLevel] Step blocked by missing require_step: step={step.Id}, require={requireId}");
                return false;
            }
        }

        foreach (string blockId in EnumerateStringSlots(step.BlockStep))
        {
            if (!string.IsNullOrEmpty(blockId) && _completedSteps.Contains(blockId))
            {
                Debug.LogWarning($"[RuleLevel] Step blocked by block_step: step={step.Id}, block={blockId}");
                return false;
            }
        }

        return true;
    }

    private void ExecuteStep(LevelRuleStep step, RuleOperation sourceOperation)
    {
        _completedSteps.Add(step.Id);
        Debug.LogWarning($"[RuleLevel] Step completed: {step.Id}, actor={step.ActorId}, target={step.TargetId}, effects={string.Join("|", step.Effect)}");

        if (step.FailLevel)
        {
            GameSet.instance.gameManager.DelGameResult(false);
            return;
        }

        ShowSuccessFeedback(step);
        GameSet.instance.gameManager.AddScore();

        if (step.SuccessHide)
        {
            HideActor(step.ActorId, sourceOperation);
        }

        RunEffectIds(step.Effect, step);
    }

    private static void ShowSuccessFeedback(LevelRuleStep step)
    {
        if (step == null || string.IsNullOrEmpty(step.SuccessTip))
        {
            return;
        }

        GameSet.instance.gameManager.ShowToast(ResolveSuccessTip(step.SuccessTip));
    }

    private static string ResolveSuccessTip(string key)
    {
        switch (key)
        {
            case "L01_SAFE_DOOR_LOCK":
                return "门锁还没扣防盗链，睡前要确认门锁、反锁和防盗链都到位。";
            case "L01_SAFE_WINDOW_OPEN":
                return "窗户还开着，夜晚独居要及时关窗，避免攀爬、窥视和安全风险。";
            case "L01_SAFE_CURTAIN_OPEN":
                return "窗帘没有拉严，房间内部容易被外面看到，隐私会暴露。";
            case "L01_SAFE_TAKEOUT_INFO":
                return "外卖袋上的姓名、电话和门牌号要及时处理，别把个人信息留在外面。";
            case "L01_SAFE_BEDSIDE_PHONE":
                return "床头手机亮屏可能暴露陌生消息或定位信息，记得检查并锁屏。";
            case "L01_SAFE_HAND_PHONE":
                return "门窗还没确认安全时不要沉迷手机，要先观察周围环境。";
            case "L01_SAFE_SOCKET_WIRE":
                return "插座和长电线存在过热、绊倒或漏电隐患，睡前要整理好。";
            case "L01_SAFE_FOOTPRINT":
                return "门口有陌生脚印，说明可能有人靠近或进入过，要提高警惕。";
            default:
                return key;
        }
    }

    private void RunEffectIds(IEnumerable<string> effectIds, LevelRuleStep step)
    {
        foreach (string effectId in effectIds)
        {
            foreach (string normalizedEffectId in SplitEffectIds(effectId))
            {
                if (_tables.TryGetEffect(normalizedEffectId, out LevelRuleEffect effect))
                {
                    RunEffect(effect, step);
                }
                else
                {
                    Debug.LogWarning($"[RuleLevel] Effect config not found: effect={normalizedEffectId}");
                }
            }
        }
    }

    private void RunEffect(LevelRuleEffect effect, LevelRuleStep step)
    {
        Debug.LogWarning($"[RuleLevel] Run effect: {effect.Id}, type={effect.EffectType}");

        if (_elementRegistry == null || !_elementRegistry.TryGet(effect.Id, out RuleElementRegistry.RuleElementEntry target))
        {
            Debug.LogWarning($"[RuleLevel] Effect target not found: effect={effect.Id}");
            return;
        }

        switch ((LevelRuleEffectType)effect.EffectType)
        {
            case LevelRuleEffectType.Show:
                RunVisibilityEffect(target, true, effect.BoolParam, effect.FloatParam);
                break;
            case LevelRuleEffectType.Hide:
                RunVisibilityEffect(target, false, effect.BoolParam, effect.FloatParam);
                break;
            case LevelRuleEffectType.SwapElement:
                SetActive(target, false);
                break;
            case LevelRuleEffectType.EnableElement:
                SetInteractable(target, true);
                break;
            case LevelRuleEffectType.DisableElement:
                SetInteractable(target, false);
                break;
            case LevelRuleEffectType.MoveElement:
                if (target != null && target.target != null)
                {
                    target.target.transform.localPosition += ParseVector(effect.VectorParam);
                }
                break;
            case LevelRuleEffectType.PlayAnimation:
                Animator animator = target.target != null ? target.target.GetComponent<Animator>() : null;
                if (animator != null && !string.IsNullOrEmpty(effect.StringParam))
                {
                    animator.Play(effect.StringParam);
                }
                break;
            case LevelRuleEffectType.SetState:
                if (!string.IsNullOrEmpty(effect.StringParam))
                {
                    _stateFlags[effect.StringParam] = effect.BoolParam;
                }
                break;
            case LevelRuleEffectType.PlayAudio:
                if (!string.IsNullOrEmpty(effect.StringParam))
                {
                    GameSet.instance.gameManager.ShowGameTip(effect.StringParam);
                }
                else
                {
                    AudioSource targetAudio = target.target != null ? target.target.GetComponent<AudioSource>() : null;
                }
                break;
        }
    }

    private static void SetActive(RuleElementRegistry.RuleElementEntry entry, bool value)
    {
        if (entry != null && entry.target != null)
        {
            entry.target.SetActive(value);
        }
    }

    private void RunVisibilityEffect(RuleElementRegistry.RuleElementEntry entry, bool visible, bool fade, IReadOnlyList<float> timing)
    {
        float delay = timing != null && timing.Count > 0 ? timing[0] : 0f;
        float duration = timing != null && timing.Count > 1 ? timing[1] : 0f;

        if (!fade || duration <= 0f)
        {
            StartCoroutine(RunDelayedAction(delay, () => SetActive(entry, visible)));
            return;
        }

        StartCoroutine(FadeVisibility(entry, visible, delay, duration));
    }

    private System.Collections.IEnumerator RunDelayedAction(float delay, System.Action action)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        action?.Invoke();
    }

    private System.Collections.IEnumerator FadeVisibility(RuleElementRegistry.RuleElementEntry entry, bool visible, float delay, float duration)
    {
        if (entry == null || entry.target == null)
        {
            yield break;
        }

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        CanvasGroup canvasGroup = entry.target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = entry.target.AddComponent<CanvasGroup>();
        }

        entry.target.SetActive(true);
        float from = visible ? 0f : canvasGroup.alpha;
        float to = visible ? 1f : 0f;
        canvasGroup.alpha = from;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        canvasGroup.alpha = to;
        if (!visible)
        {
            entry.target.SetActive(false);
        }
    }

    private static void SetInteractable(RuleElementRegistry.RuleElementEntry entry, bool value)
    {
        if (entry == null)
        {
            return;
        }

        if (entry.target == null)
        {
            return;
        }

        Collider2D collider = entry.target.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = value;
        }

        CanvasGroup canvasGroup = entry.target.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.interactable = value;
            canvasGroup.blocksRaycasts = value;
        }

        entry.target.SetActive(true);
    }

    private static Vector3 ParseVector(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Vector3.zero;
        }

        string[] parts = value.Split('|');
        float x = parts.Length > 0 ? float.Parse(parts[0]) : 0f;
        float y = parts.Length > 1 ? float.Parse(parts[1]) : 0f;
        float z = parts.Length > 2 ? float.Parse(parts[2]) : 0f;
        return new Vector3(x, y, z);
    }

    private static IEnumerable<int> EnumerateIntSlots(params int[] values)
    {
        return values;
    }

    private static IEnumerable<string> EnumerateStringSlots(params string[] values)
    {
        return values;
    }

    private static IEnumerable<string> SplitEffectIds(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield break;
        }

        string[] parts = value.Split('|');
        foreach (string part in parts)
        {
            string effectId = part.Trim();
            if (!string.IsNullOrEmpty(effectId))
            {
                yield return effectId;
            }
        }
    }

    private void HideActor(string actorId, RuleOperation sourceOperation)
    {
        if (sourceOperation != null)
        {
            sourceOperation.gameObject.SetActive(false);
            Debug.LogWarning($"[RuleLevel] Success hide source actor: actor={actorId}, object={sourceOperation.name}");
            return;
        }

        if (string.IsNullOrEmpty(actorId))
        {
            return;
        }

        if (_elementRegistry != null && _elementRegistry.TryGet(actorId, out RuleElementRegistry.RuleElementEntry entry))
        {
            SetActive(entry, false);
            Debug.LogWarning($"[RuleLevel] Success hide actor from registry: actor={actorId}");
            return;
        }

        RuleOperation[] operations = GetComponentsInChildren<RuleOperation>(true);
        foreach (RuleOperation operation in operations)
        {
            if (operation.actorId == actorId)
            {
                operation.gameObject.SetActive(false);
                Debug.LogWarning($"[RuleLevel] Success hide actor object: actor={actorId}");
                return;
            }
        }

        Debug.LogWarning($"[RuleLevel] Success hide actor not found: actor={actorId}");
    }

    private static string ResolveEffectElementId(string elementId, LevelRuleStep step)
    {
        if (step == null || string.IsNullOrEmpty(elementId))
        {
            return elementId;
        }

        switch (elementId)
        {
            case "@actor":
                return step.ActorId;
            case "@target":
                return step.TargetId;
            default:
                return elementId;
        }
    }
}
