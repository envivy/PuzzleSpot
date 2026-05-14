using System.Collections;
using DG.Tweening;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : UI
{
    public GameObject winNode;
    public GameObject loseNode;
    public Button winReturnBtn;
    public Button winContinueBtn;
    public Button loseReturnBtn;
    public Button loseNextBtn;
    public Image winLevelImage;
    public Image loseLevelImage;
    public Text winTip;
    
    #region WinCanvasGroup
    public CanvasGroup winTitleImgGroup;
    public CanvasGroup winTitleGroup;
    public CanvasGroup winStarsGroup;
    public CanvasGroup winLevelImgGroup;
    public CanvasGroup winTipGroup;
    public CanvasGroup winBtnGroup;
    #endregion
    
    #region LoseCanvasGroup
    public CanvasGroup loseTitleImgGroup;
    public CanvasGroup loseTitleGroup;
    public CanvasGroup loseLevelImgGroup;
    public CanvasGroup[] loseTips;
    private CanvasGroup _loseTipGroup;
    public CanvasGroup loseBtnGroup;
    #endregion

    private int _winStars; //获得的星星数量
    public Transform winStarsNode;
    public Transform[] allAnimStars;
    public Transform[] allTargetStars;
    public ParticleSystem[] allStarParticles;

    
    public override void ShowUI(bool isWin)
    {
        base.ShowUI(isWin);
        winNode.SetActive(false);
        loseNode.SetActive(false);
        HideAllLoseTips();
        if(isWin) ShowWin();
        else ShowLose();
    }

    private void ShowWin()
    {
        ShowWinLevelImage();
        winTitleImgGroup.alpha = 0;
        winTitleGroup.alpha = 0;
        winStarsGroup.alpha = 0;
        winLevelImgGroup.alpha = 0;
        winTipGroup.alpha = 0;
        winBtnGroup.alpha = 0;
        winReturnBtn.interactable = false;
        winContinueBtn.interactable = false;
        winTip.text = LocalizationManager.GetTermTranslation($"LevelWin{GameSet.instance.gameManager.nowLevelData.levelID}");
        for (var i = 0; i < winStarsNode.childCount; i++)
        {
            winStarsNode.GetChild(i).gameObject.SetActive(false);
        }

        _winStars = GameSet.instance.gameManager.winStarNum;
        winNode.SetActive(true);
        StartCoroutine(StartWinAnim());
    }

    private void ShowWinLevelImage()
    {
        ResourceManager.Instance.LoadResAsync<Sprite>(GameSet.instance.gameManager.nowLevelData.imagePath3, (sprite) =>
        {
            winLevelImage.sprite = sprite;
        });
    }
    
    private void ShowLose()
    {
        ShowLoseLevelImage();
        SetTipGroup();
        loseTitleImgGroup.alpha = 0;
        loseTitleGroup.alpha = 0;
        loseLevelImgGroup.alpha = 0;
        _loseTipGroup.alpha = 0;
        loseBtnGroup.alpha = 0;
        loseReturnBtn.interactable = false;
        loseNextBtn.interactable = false;
        loseNode.SetActive(true);
        StartCoroutine(StartLoseAnim());
        GameSet.instance.LogEvent("RV_ButtonShow","AD","RV_Skip2");
    }

    private void SetTipGroup()
    {
        switch (LocalizationManager.CurrentLanguage)
        {
            case "English":
                _loseTipGroup = loseTips[0];
                break;
            case "Arabic":
                _loseTipGroup = loseTips[1];
                break;
            case "Portuguese":
                _loseTipGroup = loseTips[2];
                break;
            case "Spanish":
                _loseTipGroup = loseTips[3];
                break;
        }
    }

    private void HideAllLoseTips()
    {
        foreach (var loseTip in loseTips)
        {
            loseTip.gameObject.SetActive(false);
        }
    }
    
    private void ShowLoseLevelImage()
    {
        ResourceManager.Instance.LoadResAsync<Sprite>(GameSet.instance.gameManager.nowLevelData.imagePath2, (sprite) =>
        {
            loseLevelImage.sprite = sprite;
        });
    }

    private IEnumerator StartWinAnim()
    {
        yield return new WaitForSeconds(0.5f);
        winTitleImgGroup.DOFade(1, 0.2f);
        yield return new WaitForSeconds(0.3f);
        winTitleGroup.DOFade(1, 0.3f);
        winStarsGroup.DOFade(1, 0.3f);
        winLevelImgGroup.DOFade(1, 0.3f);
        yield return new WaitForSeconds(0.3f);
        winTipGroup.DOFade(1, 0.3f);
        yield return new WaitForSeconds(1f);
        winBtnGroup.DOFade(1, 0.5f);
        winReturnBtn.interactable = true;
        winContinueBtn.interactable = true;
        yield return new WaitForSeconds(1f);
        
        if(_winStars < 1) yield break;
        //展示第一颗星星
        allAnimStars[0].localScale = new Vector3(3f, 3f, 3f);
        allAnimStars[0].gameObject.SetActive(true);
        allAnimStars[0].DOLocalMove(new Vector3(allTargetStars[0].localPosition.x, allTargetStars[0].localPosition.y, 0), 0.5f);
        allAnimStars[0].DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f);
        allAnimStars[0].DORotate(new Vector3(0f, 0f, 30f), 0.5f);
        yield return new WaitForSeconds(0.3f);
        allStarParticles[0].gameObject.SetActive(true);
        allStarParticles[0].Play();
        GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.starAudio);
        yield return new WaitForSeconds(0.1f);
        
        if(_winStars < 2) yield break;
        //展示第二颗星星
        allAnimStars[1].localScale = new Vector3(3f, 3f, 3f);
        allAnimStars[1].gameObject.SetActive(true);
        allAnimStars[1].DOLocalMove(new Vector3(allTargetStars[1].localPosition.x, allTargetStars[1].localPosition.y, 0), 0.5f);
        allAnimStars[1].DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f);
        yield return new WaitForSeconds(0.3f);
        allStarParticles[1].gameObject.SetActive(true);
        allStarParticles[1].Play();
        GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.starAudio);
        yield return new WaitForSeconds(0.1f);
        
        if(_winStars < 3) yield break;
        //展示第三颗星星
        allAnimStars[2].localScale = new Vector3(3f, 3f, 3f);
        allAnimStars[2].gameObject.SetActive(true);
        allAnimStars[2].DOLocalMove(new Vector3(allTargetStars[2].localPosition.x, allTargetStars[2].localPosition.y, 0), 0.5f);
        allAnimStars[2].DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f);
        allAnimStars[2].DORotate(new Vector3(0f, 0f, -30f), 0.5f);
        yield return new WaitForSeconds(0.3f);
        allStarParticles[2].gameObject.SetActive(true);
        allStarParticles[2].Play();
        GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.starAudio);
    }

    private IEnumerator StartLoseAnim()
    {
        yield return new WaitForSeconds(0.5f);
        loseTitleImgGroup.DOFade(1, 0.2f);
        yield return new WaitForSeconds(0.3f);
        loseTitleGroup.DOFade(1, 0.3f);
        loseLevelImgGroup.DOFade(1, 0.3f);
        yield return new WaitForSeconds(0.3f);
        _loseTipGroup.gameObject.SetActive(true);
        _loseTipGroup.DOFade(1, 0.3f);
        yield return new WaitForSeconds(1f);
        loseBtnGroup.DOFade(1, 0.5f);
        loseReturnBtn.interactable = true;
        loseNextBtn.interactable = true;
    }

    public void OnClickReturnBtn()
    {
        HideUI();
        GameSet.instance.gameManager.ShowLoading(false,() =>
        {
            GameSet.instance.gameManager.mainPageUI.ShowUI();
        });
        GameSet.instance.gameManager.gameUI.HideUI();
    }

    public void OnClickRetryBtn()
    {
        GameSet.instance.gameManager.StartGame(GameSet.instance.gameManager.nowLevelData.levelID);
        GameSet.instance.PlayInsertAD("IV_Fail");
        loseNode.gameObject.SetActive(false);
        _loseTipGroup.gameObject.SetActive(false);
        HideUI();
        GameSet.instance.gameManager.gameUI.HideUI();
    }

    public void OnClickContinueBtn()
    {
        GameSet.instance.PlayInsertAD("IV_Succ");
        if (GameSet.instance.gameManager.nowLevel.levelID + 1 > GameSet.instance.matter.levels.Count)
        {
            GameSet.instance.gameManager.gameUI.HideUI();
            GameSet.instance.gameManager.StartGame(1);
            HideUI();
            return;
        }
        GameSet.instance.gameManager.gameUI.HideUI();
        GameSet.instance.gameManager.StartGame(GameSet.instance.gameManager.nowLevel.levelID + 1);
        HideUI();
    }

    public void OnClickVideoNextBtn()
    {
        GameSet.instance.PlayVideoAD("RV_Skip2", () =>
        {
            loseNode.SetActive(false);
            _loseTipGroup.gameObject.SetActive(false);
            GameSet.instance.gameManager.SaveLevelFinishInfo(GameSet.instance.gameManager.nowLevelData.levelID, 1);
            GameSet.instance.gameManager.SaveLevelFinishInfo(GameSet.instance.gameManager.nowLevelData.levelID + 1, 0);
            //直接跳到下一关
            if (GameSet.instance.gameManager.nowLevel.levelID + 1 > GameSet.instance.matter.levels.Count)
            {
                GameSet.instance.gameManager.gameUI.HideUI();
                GameSet.instance.gameManager.StartGame(1);
                HideUI();
                return;
            }
            GameSet.instance.gameManager.gameUI.HideUI();
            GameSet.instance.gameManager.StartGame(GameSet.instance.gameManager.nowLevel.levelID + 1);
            HideUI();
        });
    }
}
