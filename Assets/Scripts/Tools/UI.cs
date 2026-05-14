using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    public Transform allUI;
    public CanvasGroup bg;

    public virtual void ShowUI()
    {
        UIAni();
    }

    public virtual void ShowUI(bool isWin)
    {
        UIAni();
    }

    public virtual void ShowUI(Action YesCall, Action NoCall)
    {
        UIAni();
    }

    public virtual void ShowUI(string msg)
    {
        UIAni();
    }
    
    public virtual void ShowUI(Action callBack)
    {
        UIAni();
    }

    private void UIAni()
    {
        gameObject.SetActive(true);
        bg.alpha = 0;
        allUI.localScale = Vector3.zero;
        bg.DOFade(1, 0.2f);
        allUI.DOScale(1.1f, 0.2f).OnComplete(() =>
        {
            allUI.DOScale(1, 0.1f);
        });
    }

    public virtual void HideUI()
    {
        gameObject.SetActive(false);
    }

    public virtual void CloseMe()
    {
        GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.CloseBtnAudio);
        bg.alpha = 1;
        bg.DOFade(0, 0.2f);
        allUI.DOScale(0, 0.2f).OnComplete(() =>
        {
            HideUI();
        });

    }
}
