using UnityEngine;

public class RuleTarget : MonoBehaviour
{
    public string targetId;
    public int priority;

    private void Awake()
    {
        RuleInteractionAutoSetup.SetupTarget(gameObject);
    }
}
