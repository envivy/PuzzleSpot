using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class MainPageUI : UI
{
    public Transform content;
    private List<LevelItem> _levelItems = new List<LevelItem>();
    public Text starsTxt;
    public CanvasGroup mask;

    public CanvasGroup settingNode;
    public CanvasGroup settingCanvas;
    
    #region 广告奖励
    public CanvasGroup rewardNode;
    public Image rewardProgress;
    public GameObject noClaim;
    public Button claimBtn;
    #endregion

    #region 提前解锁
    public CanvasGroup preUnlockNode;
    public Image levelImage;
    public Text unlockLevelTxt;
    private int _preUnlockLevel;
    #endregion

    #region 新手引导

    public GameObject teachNode;
    public GameObject finger1;

    #endregion
    
    
    private void Start()
    {
        GameSet.instance.gameManager.OnEndTeaching = HideTeach;
    } 

    public override void ShowUI()
    {
        gameObject.SetActive(true);
        GameSet.instance.LogEvent("Home_Show");
        GameSet.instance.gameManager.PlayBGM(GameSet.instance.matter.mainPageAudio);
        ShowLevels();
        if (GameSet.instance.gameManager.isShowVideoReward)
        {
            ShowVideoReward();
            GameSet.instance.gameManager.isShowVideoReward = false;
        }
        starsTxt.text = GameSet.instance.userData.Stars.ToString();
        GameSet.instance.gameManager.OnPreUnlock = ShowPreUnlock;
        if (GameSet.instance.userData.LevelFinish.Count == 1 && !GameSet.instance.userData.IsFinishTeach)
        {
            ShowTeach();
        }
    }

    public override void HideUI()
    {
        base.HideUI();
        GameSet.instance.gameManager.BGM.Stop();
    }

    private void ShowMask()
    {
        mask.gameObject.SetActive(true);
        mask.alpha = 0;
        mask.DOFade(1, 0.4f);
    }

    /// <summary>
    /// 展示关卡
    /// </summary>
    private void ShowLevels()
    {
        if (_levelItems.Count > 0)
        {
            foreach (var item in _levelItems)
            {
                item.UpdateState();
            }
        }
        else
        {
            for (var i = 0; i < GameSet.instance.gameManager.GetTotalLevelCount(); i++)
            {
                _levelItems.Add(Instantiate(ResourceManager.Instance.LoadPrefabSync<LevelItem>("LevelItem"), content));
                _levelItems[i].levelID = i + 1;
            }
        }
    }

    /// <summary>
    /// 展示广告奖励
    /// </summary>
    public void ShowVideoReward()
    {
        ShowMask();
        noClaim.SetActive(!Mathf.Approximately(GameSet.instance.userData.VideoProgress, 1));
        claimBtn.gameObject.SetActive(Mathf.Approximately(GameSet.instance.userData.VideoProgress, 1));
        rewardNode.transform.localScale = Vector3.zero;
        rewardNode.alpha = 1;
        rewardProgress.fillAmount = GameSet.instance.userData.VideoProgress;
        rewardNode.gameObject.SetActive(true);
        rewardNode.transform.DOScale(1.1f, 0.4f).OnComplete(() =>
        {
            rewardNode.transform.DOScale(1, 0.1f);
        });
    }

    /// <summary>
    /// 领取广告奖励
    /// </summary>
    public void OnClickVideoReward()
    {
        GameSet.instance.userData.HintCount++;
        GameSet.instance.userData.VideoProgress = 0;
        GameSet.instance.SaveUserData();
        OnClickCloseReward();
    }

    /// <summary>
    /// 展示提前解锁
    /// </summary>
    private void ShowPreUnlock(int levelID)
    {
        ShowMask();
        ShowLevelImage(levelID);
        GameSet.instance.LogEvent("RV_ButtonShow","AD","RV_UnlockLevels");
        _preUnlockLevel = levelID;
        unlockLevelTxt.text = LocalizationManager.GetTermTranslation("LEVEL") + " " + levelID;
        preUnlockNode.transform.localScale = Vector3.zero;
        preUnlockNode.alpha = 1;
        preUnlockNode.gameObject.SetActive(true);
        preUnlockNode.transform.DOScale(1.1f, 0.4f).OnComplete(() =>
        {
            preUnlockNode.transform.DOScale(1, 0.1f);
        });
    }

    private void ShowLevelImage(int levelID)
    {
        var levelData = GameSet.instance.gameManager.GetLevelData(levelID);
        if (levelData == null || string.IsNullOrEmpty(levelData.imagePath1)) return;
        ResourceManager.Instance.LoadResAsync<Sprite>(levelData.imagePath1, (sprite) =>
        {
            levelImage.sprite = sprite;
        });
    }

    #region 新手引导流程
    
    /// <summary>
    /// 新手引导
    /// </summary>
    private void ShowTeach()
    {
        GameSet.instance.gameManager.isTeaching = true;
        teachNode.SetActive(true);
        Invoke(nameof(ShowFinger),0.5f);
    }

    private void HideTeach()
    {
        teachNode.SetActive(false);
    }

    private void ShowFinger()
    {
        finger1.gameObject.SetActive(true);
    }
    
    #endregion
    
    /// <summary>
    /// 关闭提前解锁
    /// </summary>
    public void ClosePreUnlock()
    {
        preUnlockNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            preUnlockNode.gameObject.SetActive(false);
        });
        mask.DOFade(0, 0.3f).OnComplete(() =>
        {
            mask.gameObject.SetActive(false);
        });
    }

    public void OnClickPreUnlock()
    {
        ClosePreUnlock();
        GameSet.instance.PlayVideoAD("RV_UnlockLevels", () =>
        {
            GameSet.instance.gameManager.SaveLevelFinishInfo(_preUnlockLevel, 0);
            ShowLevels();
        });
    }

    public void OnClickCloseReward()
    {
        rewardNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            rewardNode.gameObject.SetActive(false);
        });
        mask.DOFade(0, 0.3f).OnComplete(() =>
        {
            mask.gameObject.SetActive(false);
        });
    }
    
    public void OpenSettingClick() {
        settingCanvas.transform.localScale = Vector3.zero;
        settingNode.alpha = 1;
        settingCanvas.alpha = 1;
        settingCanvas.transform.DOScale(1.1f, 0.4f).OnComplete(() =>
        {
            settingCanvas.transform.DOScale(1, 0.1f);
        });
        settingNode.gameObject.SetActive(true);
    }

    public void CloseSetting()
    {
        settingNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            settingNode.gameObject.SetActive(false);
        });
    }

    public void OpenStoreClick() {
        GameSet.instance.gameManager.storeUI.ShowUI();
    }
    
    public void StartGameClick() {
    }
}
