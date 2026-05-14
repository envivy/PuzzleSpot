using System.Globalization;
using BX;
using DG.Tweening;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : UI
{
    public JoysticksPanel touch;
    public CanvasGroup tipGroup;
    public Text title;
    public Text timeTxt;
    public Text progressTxt;
    public Text tipTxt;
    public Image progressImg;
    public CanvasGroup winCheck;
    public CanvasGroup mask;
    public GameObject gameOverMask; //关卡结束遮罩

    #region Hint
    public Text hintTxt;
    public Text hintNodeTxt;
    public Text hintTipTxt;
    public Text answerTipTxt;
    public Text hintStepTip;
    public GameObject hintADIcon;
    public CanvasGroup hintNode;
    public CanvasGroup hintTipNode;
    public CanvasGroup answerTipNode;
    public CanvasGroup reviveNode;
    public GameObject hintFinger1;
    public GameObject hintFinger2;
    public ScrollRect answerScrollRect;
    #endregion
    
    #region SkipLevel
    public CanvasGroup skipLevelNode;
    #endregion

    #region Restart
    public CanvasGroup restartNode;
    #endregion
    
    private float _time;

    private void Start()
    {
        GameSet.instance.gameManager.OnScoreChange = RefreshProgress;
        GameSet.instance.gameManager.OnWinCheck = ShowWinCheck;
        GameSet.instance.gameManager.OnShowHintTeach = ShowHintTeach;
        GameSet.instance.gameManager.OnShowGameTip = ShowTip;
        GameSet.instance.gameManager.OnShowGameMask = ShowGameMask;
    }

    public override void ShowUI()
    {
        base.ShowUI();
        ShowLevelInfo();
        gameOverMask.SetActive(false);
        _time = GameSet.instance.gameManager.nowLevelData.time;
        timeTxt.text = _time.ToString(CultureInfo.InvariantCulture);
        UpdateHintCountShow();
        ShowTip();
    }

    public override void HideUI()
    {
        base.HideUI();
        hintNode.gameObject.SetActive(false);
        hintTipNode.gameObject.SetActive(false);
        answerTipNode.gameObject.SetActive(false);
        restartNode.gameObject.SetActive(false);
        skipLevelNode.gameObject.SetActive(false);
        reviveNode.gameObject.SetActive(false);
        mask.gameObject.SetActive(false);
    }

    private void ShowMask()
    {
        mask.gameObject.SetActive(true);
        mask.alpha = 0;
        mask.DOFade(1, 0.4f);
    }

    private void ShowLevelInfo()
    {
        title.text = LocalizationManager.GetTermTranslation($"LevelTitle{GameSet.instance.gameManager.nowLevelData.levelID}");
        tipTxt.text = LocalizationManager.GetTermTranslation($"LevelTip{GameSet.instance.gameManager.nowLevelData.levelID}");
        progressImg.fillAmount = 0;
        progressTxt.text = $"0/{GameSet.instance.gameManager.nowLevelData.targetScore}";
    }
    
    private void RefreshProgress()
    {
        progressImg.fillAmount = (float)GameSet.instance.gameManager.curScore / (float)GameSet.instance.gameManager.nowLevelData.targetScore;
        progressTxt.text = $"{GameSet.instance.gameManager.curScore}/{GameSet.instance.gameManager.nowLevelData.targetScore}";
    } 

    private void ShowTip()
    {
        if (string.IsNullOrEmpty(tipTxt.text))
        {
            tipGroup.alpha = 0;
        }
        else
        {
            tipGroup.alpha = 1;
            Invoke(nameof(HideTip),3);
        }
    }

    private void ShowTip(string tip)
    {
        if(string.IsNullOrEmpty(tip)) return;
        tipTxt.text = tip;
        tipGroup.alpha = 1;
        Invoke(nameof(HideTip),3);
    }
    
    private void HideTip()
    {
        tipGroup.DOFade(0,0.5f);
        tipTxt.text = "";
    }
    
    #region 提示
    
    public void OnClickShowHint()
    {
        hintFinger1.gameObject.SetActive(false);
        GameSet.instance.gameManager.StopGameTimer(false);
        hintADIcon.SetActive(GameSet.instance.userData.HintCount <= 0);
        if (GameSet.instance.userData.HintCount <= 0) GameSet.instance.LogEvent("RV_ButtonShow","AD","RV_Prompt");
        GameSet.instance.LogEvent("RV_ButtonShow","AD","RV_Answer");
        UpdateHintCountShow();
        ShowMask();
        hintNode.transform.localScale = Vector3.zero;
        hintStepTip.text = GameSet.instance.gameManager.GetTip();
        hintNode.alpha = 1;
        hintNode.gameObject.SetActive(true);
        hintNode.transform.DOScale(1.1f, 0.2f).OnComplete(() =>
        {
            hintNode.transform.DOScale(1, 0.1f);
        });
    }

    public void OnClickCloseHint(bool closeMask)
    {
        hintFinger2.gameObject.SetActive(false);
        hintNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            hintNode.gameObject.SetActive(false);
            if(closeMask) GameSet.instance.gameManager.StartGameTimer();
        });
        if (closeMask)
        {
            mask.DOFade(0, 0.3f).OnComplete(() =>
            {
                mask.gameObject.SetActive(false);
            });
        }
    }

    public void OnClickHintBtn()
    {
        //提示数量不足，看广告获取
        if (GameSet.instance.userData.HintCount <= 0)
        {
            GameSet.instance.PlayVideoAD("RV_Prompt", () =>
            {
                OnClickCloseHint(false);
                hintTipNode.transform.localScale = Vector3.zero;
                hintTipNode.alpha = 1;
                hintTipNode.gameObject.SetActive(true);
                hintTipNode.transform.DOScale(1.1f, 0.2f).OnComplete(() =>
                {
                    hintTipNode.transform.DOScale(1, 0.1f);
                });
            });
        }
        else
        {
            GameSet.instance.userData.HintCount--;
            GameSet.instance.SaveUserData();
            OnClickCloseHint(false);
            UpdateHintCountShow();
            hintTipNode.transform.localScale = Vector3.zero;
            hintTipNode.alpha = 1;
            hintTipNode.gameObject.SetActive(true);
            hintTipNode.transform.DOScale(1.1f, 0.2f).OnComplete(() =>
            {
                hintTipNode.transform.DOScale(1, 0.1f);
            });
        }
    }

    public void OnClickCloseHintTip()
    {
        hintTipNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            hintTipNode.gameObject.SetActive(false);
            GameSet.instance.gameManager.StartGameTimer();
        });
        mask.DOFade(0, 0.3f).OnComplete(() =>
        {
            mask.gameObject.SetActive(false);
        });
    }

    public void OnClickCloseAnswerTip()
    {
        answerTipNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            answerTipNode.gameObject.SetActive(false);
            answerScrollRect.verticalNormalizedPosition = 1f;
            GameSet.instance.gameManager.StartGameTimer();
        });
        mask.DOFade(0, 0.3f).OnComplete(() =>
        {
            mask.gameObject.SetActive(false);
        });
    }

    public void OnClickAnswerBtn()
    {
        GameSet.instance.PlayVideoAD("RV_Hint", () =>
        {
            OnClickCloseHint(false);
            UpdateHintCountShow();
            answerTipNode.transform.localScale = Vector3.zero;
            answerTipNode.alpha = 1;
            answerTipNode.gameObject.SetActive(true);
            answerTipNode.transform.DOScale(1.1f, 0.2f).OnComplete(() =>
            {
                answerTipNode.transform.DOScale(1, 0.1f);
            });
        });
    }

    private void UpdateHintCountShow()
    {
        hintTxt.text = $"x{GameSet.instance.userData.HintCount}";
        hintNodeTxt.text =  $"x{GameSet.instance.userData.HintCount}";
        hintTipTxt.text =  $"{GameSet.instance.userData.HintCount}";
        answerTipTxt.text = $"{GameSet.instance.userData.HintCount}";
    }
    
    private void ShowHintTeach()
    {
        hintFinger1.gameObject.SetActive(true);
        hintFinger2.gameObject.SetActive(true);
    }
    
    #endregion

    #region 复活加时

    private void ShowRevive()
    {
        if(!GameSet.instance.gameManager.isGameStart) return;
        ShowMask();
        reviveNode.transform.localScale = Vector3.zero;
        reviveNode.alpha = 1;
        reviveNode.gameObject.SetActive(true);
        GameSet.instance.LogEvent("RV_ButtonShow","AD","RV_Overtime");
        reviveNode.transform.DOScale(1.1f, 0.4f).OnComplete(() =>
        {
            reviveNode.transform.DOScale(1, 0.1f);
        });
    }
    
    public void OnClickReviveBtn()
    {
        GameSet.instance.PlayVideoAD("RV_Overtime", () =>
        {
            _time += 180;
            GameSet.instance.gameManager.StartGameTimer();
        }, () => { GameSet.instance.gameManager.DelGameResult(false);});
        reviveNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            reviveNode.gameObject.SetActive(false);
        });
        mask.DOFade(0, 0.3f).OnComplete(() =>
        {
            mask.gameObject.SetActive(false);
        });
    }

    public void OnClickCloseRevive()
    {
        reviveNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            reviveNode.gameObject.SetActive(false);
            GameSet.instance.gameManager.DelGameResult(false);
        });
        mask.DOFade(0, 0.3f).OnComplete(() =>
        {
            mask.gameObject.SetActive(false);
        });
    }
    
    #endregion

    #region 跳关

    public void ShowSkipLevel()
    {
        GameSet.instance.gameManager.StopGameTimer(false);
        ShowMask();
        GameSet.instance.LogEvent("RV_ButtonShow","AD","RV_Skip1");
        skipLevelNode.transform.localScale = Vector3.zero;
        skipLevelNode.alpha = 1;
        skipLevelNode.gameObject.SetActive(true);
        skipLevelNode.transform.DOScale(1.1f, 0.2f).OnComplete(() =>
        {
            skipLevelNode.transform.DOScale(1, 0.1f);
        });
    }

    public void OnClickSkipLevel()
    {
        skipLevelNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            skipLevelNode.gameObject.SetActive(false);
        });
        GameSet.instance.PlayVideoAD("RV_Skip1", () =>
        {
            GameSet.instance.LevelEnd(GameSet.instance.gameManager.nowLevelData.levelID,BXSdk.StageResult.StageSkip);
            GameSet.instance.gameManager.DelGameResult(true,1);
        });
        mask.DOFade(0, 0.3f).OnComplete(() =>
        {
            mask.gameObject.SetActive(false);
        });
    }

    public void CloseSkipLevel()
    {
        skipLevelNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            skipLevelNode.gameObject.SetActive(false);
            GameSet.instance.gameManager.StartGameTimer();
        });
        mask.DOFade(0, 0.3f).OnComplete(() =>
        {
            mask.gameObject.SetActive(false);
        });
    }

    #endregion

    #region 重玩
    
    public void ShowRestartGame()
    {
        GameSet.instance.gameManager.StopGameTimer(false);
        GameSet.instance.LogEvent("RV_ButtonShow","AD","RV_Restart");
        ShowMask();
        restartNode.transform.localScale = Vector3.zero;
        restartNode.alpha = 1;
        restartNode.gameObject.SetActive(true);
        restartNode.transform.DOScale(1.1f, 0.2f).OnComplete(() =>
        {
            restartNode.transform.DOScale(1, 0.1f);
        });
    }

    public void OnClickRestart()
    {
        restartNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            restartNode.gameObject.SetActive(false);
        });
        GameSet.instance.gameManager.StopGameTimer(true);
        GameSet.instance.LevelEnd(GameSet.instance.gameManager.nowLevelData.levelID,BXSdk.StageResult.StageFail);
        GameSet.instance.PlayVideoAD("RV_Restart", () =>
        {
            GameSet.instance.gameManager.StartGame(GameSet.instance.gameManager.nowLevelData.levelID);
            HideUI();
        });
        mask.DOFade(0, 0.3f).OnComplete(() =>
        {
            mask.gameObject.SetActive(false);
        });
    }

    public void CloseRestart()
    {
        restartNode.DOFade(0, 0.3f).OnComplete(() =>
        {
            restartNode.gameObject.SetActive(false);
            GameSet.instance.gameManager.StartGameTimer();
        });
        mask.DOFade(0, 0.3f).OnComplete(() =>
        {
            mask.gameObject.SetActive(false);
        });
    }

    #endregion

    /// <summary>
    /// 返回主界面
    /// </summary>
    public void ReturnHome()
    {
        GameSet.instance.PlayInsertAD("IV_Return");
        GameSet.instance.gameManager.StopGameTimer(true);
        GameSet.instance.gameManager.ShowLoading(false,() =>
        {
            GameSet.instance.LevelEnd(GameSet.instance.gameManager.nowLevelData.levelID,BXSdk.StageResult.StageBack);
            GameSet.instance.gameManager.mainPageUI.ShowUI();
        });
        HideUI();
    }
    
    private void Update()
    {
        if (!GameSet.instance.gameManager || !GameSet.instance.gameManager.startTimer) return;
        _time -= Time.deltaTime;
        timeTxt.text = ((int)_time).ToString();
        // 倒计时结束
        if (!(_time <= 0)) return;
        _time = 0;
        OnTimerFinished();
    }

    /// <summary>
    /// 计时结束
    /// </summary>
    private void OnTimerFinished()
    {
        ShowRevive();
        GameSet.instance.gameManager.StopGameTimer(false);
    }
    
    private void ShowWinCheck()
    {
        winCheck.alpha = 0.2f;
        winCheck.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        winCheck.gameObject.SetActive(true);
        winCheck.transform.DOScale(new Vector3(0.6f, 0.6f, 0.6f), 1f);
        winCheck.DOFade(1, 1f);
        GameSet.instance.gameManager.StopGameTimer(false);
        reviveNode.gameObject.SetActive(false);
        mask.gameObject.SetActive(false);
        Invoke(nameof(ShowWin),2f);
    }

    private int GetWinStarsCount()
    {
        if (_time < 30f) return 1;
        return _time < 60f ? 2 : 3;
    }
    
    private void ShowWin()
    {
        winCheck.gameObject.SetActive(false);
        GameSet.instance.gameManager.DelGameResult(true,GetWinStarsCount());
        GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.winAudio);
    }

    private void ShowGameMask()
    {
        gameOverMask.SetActive(true);
    }
}
