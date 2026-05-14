using UnityEngine;

public class RuleLevelBootstrap : MonoBehaviour
{
    public int levelId;
    public RuleLevelController controller;

    private void Awake()
    {
        if (controller != null)
        {
            controller.levelID = levelId;
        }
    }
}
