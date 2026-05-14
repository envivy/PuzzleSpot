using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIShowEffect : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float delayTime = 2f;
    private Button _buttonComponent;

    private void Awake()
    {
        _buttonComponent = GetComponent<Button>();
    }

    private void OnEnable()
    {
        canvasGroup.alpha = 0;
        if (_buttonComponent != null) _buttonComponent.interactable = false;
        Invoke(nameof(Show),delayTime);
    }

    private void OnDisable()
    {
        canvasGroup.alpha = 0;
    }

    private void Show()
    {
        canvasGroup.DOFade(1, 0.5f).OnComplete(() =>
        {
            if (_buttonComponent != null) _buttonComponent.interactable = true;
        });
    }
}
