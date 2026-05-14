using UnityEngine;

public class RuleGameUIBridge : MonoBehaviour
{
    public RuleLevelController controller;

    public string GetTipText()
    {
        return controller != null ? controller.GetNextHintText() : string.Empty;
    }
}
