using System;
using System.Collections;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : UI
{
    private const float LoadTime = 3f;
    public Image loadBar;
    public Image levelImage;
    private Action _onLoadComplete;
    private CyclePoint _cyclePoint;
    public bool isGameLoading;
    public RectTransform layout;
    public HorizontalLayoutGroup horizontal;
    public GameObject[] logo;

    public override void ShowUI(Action onFinished)
    {
        if (isGameLoading) ShowLevelImage();
        else ShowLogo();
        gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
        horizontal.enabled = false;
        horizontal.enabled = true;
        _onLoadComplete = onFinished;
        StartCoroutine(LoadProgressBar());
    }

    private void ShowLevelImage()
    {
        ResourceManager.Instance.LoadResAsync<Sprite>(GameSet.instance.gameManager.nowLevelData.imagePath2, (sprite) =>
        {
            levelImage.sprite = sprite;
        });
    }
    
    private void ShowLogo()
    {
        foreach (var l in logo)
        {
            l.SetActive(false);
        }
        switch (LocalizationManager.CurrentLanguage)
        {
            case "English":
                logo[0].SetActive(true);
                break;
            case "Arabic":
                logo[1].SetActive(true);
                break;
            case "Portuguese":
                logo[2].SetActive(true);
                break;
            case "Spanish":
                logo[3].SetActive(true);
                break;
        }
    }
    
    private IEnumerator LoadProgressBar()
    {
        var elapsedTime = 0f;
        var startValue = 0f;
        var endValue = 1f;
        while (elapsedTime < LoadTime)
        {
            var progress = Mathf.Lerp(startValue, endValue, elapsedTime / LoadTime);
            loadBar.fillAmount = progress;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        loadBar.fillAmount = endValue;
        HideUI();
    }

    public override void HideUI()
    {
        base.HideUI();
        _onLoadComplete?.Invoke();
    }
}
