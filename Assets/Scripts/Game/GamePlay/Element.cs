using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum ShowType
{
    None,
    Fade,
    Move,
    Scale
}

public enum HideType
{
    None,
    Fade,
    Move,
    Scale
}

public class Element : MonoBehaviour
{
    public int elementID;
    public List<int> enableOperateIDList = new List<int>();
    public new Collider2D collider;
    [HideInInspector] public bool isFinished;

    /// <summary>
    /// 当操作完成
    /// </summary>
    public void OnOperateFinished()
    {
        if(!GameSet.instance.gameManager.isGameStart || isFinished) return;
        isFinished = true;
        if (collider != null) collider.enabled = false;
        GameSet.instance.gameManager.nowLevel.RefreshUIState(elementID);
    }

    public void OnShow(ShowType showType,float moveValue = 0)
    {
        gameObject.SetActive(true);
        switch (showType)
        {
            case ShowType.Fade:
                var canvasGroup = gameObject.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();

                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1, 0.5f);
                break;
            case ShowType.Move:
                transform.DOLocalMoveX(moveValue, 0.5f);
                break;
            case ShowType.Scale:
                transform.DOScale(1, 0.3f);
                break;
        }
    }

    public void OnHide(HideType hideType, Action closeBack = null, float moveValue = 0)
    {
        switch (hideType)
        {
            case HideType.None:
                gameObject.SetActive(false);
                closeBack?.Invoke();
                break;
            
            case HideType.Fade:
                var canvasGroup = gameObject.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();

                canvasGroup.alpha = 1;
                canvasGroup.DOFade(0, 0.5f).OnComplete(() =>
                {
                    closeBack?.Invoke();
                    gameObject.SetActive(false);
                });
                break;
            case HideType.Move:
                transform.DOLocalMoveX(moveValue, 0.5f).OnComplete(() =>
                {
                    closeBack?.Invoke();
                    gameObject.SetActive(false);
                });
                break;
            case HideType.Scale:
                transform.DOScale(0, 0.3f).OnComplete(() =>
                {
                    closeBack?.Invoke();
                    gameObject.SetActive(false);
                });
                break;
        }
    }
}
