using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BX;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using I2.Loc;

public class GameManager : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("EditorTools/DestoryColl", false, 1)]
    static void DestoryColl()
    {
        if (Selection.gameObjects.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            var count = Selection.gameObjects[i].transform.childCount;

            for (int j = 0; j < count; j++)
            {
                var aaaaaaa = Selection.gameObjects[i].transform.GetChild(j).GetComponentsInChildren<Collider>();
                for (int k = 0; k < aaaaaaa.Length; k++)
                {
                    DestroyImmediate(aaaaaaa[k]);
                }
            }
        }

    }
#endif

    public UI mainPageUI, gameUI, settingUI, storeUI, gameOverUI,loadingUI,mainLoadingUI,reviewUI;
    public AudioSource BGM;
    public Transform CashNode;
    public Text CashLab;
    public Player player;
    [HideInInspector] public Level nowLevel;
    public LevelData nowLevelData;
    public Transform TopUINode;
    public Transform content;
    public List<BtnVisibleController> allVisibleBtns = new List<BtnVisibleController>();
    private readonly List<int> _stepIndex = new List<int>();

    [HideInInspector] public int curScore;
    [HideInInspector] public int winStarNum;
    [HideInInspector] public bool startTimer;
    [HideInInspector] public bool isGameStart;
    [HideInInspector] public bool isShowVideoReward;
    [HideInInspector] public bool isTeaching;
    [HideInInspector] public bool teachTimer; //记录20秒内无操作
    [HideInInspector] public bool enablePlayStepAudio = true;
    
    public Action OnScoreChange;
    public Action OnWinCheck;
    public Action<int> OnPreUnlock;
    public Action OnShowHintTeach;
    public Action<string> OnShowGameTip;
    public Action OnEndTeaching;
    public Action OnShowGameMask;

    private RuleLevelTables _ruleLevelTables;

    private void HideAllUI() {
        mainPageUI.HideUI();
        gameUI.HideUI();
        settingUI.HideUI();
        storeUI.HideUI();
        gameOverUI.HideUI();
    }

    private void Awake()
    {
        GameSet.instance.gameManager = this;
        GameSet.instance.SetGameConfig();
    }

    private void Start()
    {
        cashV = GameSet.instance.userData.Coin;
        HideAllUI();
        mainPageUI.ShowUI();
        SetBGMState();
    }

    public void ShowToast(string msg)
    {
        Instantiate(GameSet.instance.matter.toastUI, TopUINode).ShowUI(msg);
    }

    private int cashV;
    private bool canUpDateCash = true;

    public void CanUpDateCoin() {
        DOTween.To(() => cashV, x => cashV = x, GameSet.instance.userData.Coin, 1).OnUpdate(() =>
        {
            CashLab.text = cashV.ToString();
        }).OnComplete(() =>
        {
            canUpDateCash = true;
            cashV = GameSet.instance.userData.Coin;
        });
    }
    private void FixedUpdate()
    {
        if (canUpDateCash) {
            CashLab.text = GameSet.instance.userData.Coin.ToString();
        }
    }
    public void SetBGMState()
    {
        if (GameSet.instance.userData.setData.music)
        {
            BGM.Play();
        }
        else
        {
            BGM.Stop();
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        BGM.clip = clip;
        SetBGMState();
    }

    public void FlyObj(Transform what, int value, Vector3 startPos, Vector3 endPos, System.Action call)
    {
        value = value > 30 ? 30 : value;
        for (int i = 0; i < value; i++)
        {
            int index = i;
            ExtendFun.DelayDoFrame(this, index, () =>
            {
                Transform coin = Instantiate(what, TopUINode);
                coin.position = startPos;
                Vector3 dealPos = new Vector3(startPos.x + UnityEngine.Random.Range(-200, 200), startPos.y + UnityEngine.Random.Range(-200, 200), 0);
                coin.DOMove(dealPos, 0.5f).OnComplete(() => {
                    coin.DOMove(endPos, 1).OnComplete(() =>
                    {
                        if (index == (value - 1))
                        {
                            call?.Invoke();
                        }
                        Destroy(coin.gameObject);
                    });
                });
            });
        }
    }
    
    /// <summary>
    /// 展示加载页面
    /// </summary>
    public void ShowLoading(bool isGameLoading,Action onLoadFinish = null)
    {
        if(isGameLoading) loadingUI.ShowUI(onLoadFinish);
        else mainLoadingUI.ShowUI(onLoadFinish);
    }
    
    public void CleanNode(Transform node) {
        for (int i = 0; i < node.childCount; i++)
        {
            Destroy(node.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 提前解锁关卡
    /// </summary>
    /// <param name="levelID"></param>
    public void PreUnlockLevel(int levelID)
    {
        OnPreUnlock?.Invoke(levelID);
    }

    /// <summary>
    /// 提示教学
    /// </summary>
    public void ShowHintTeach()
    {
        OnShowHintTeach?.Invoke();
    }

    /// <summary>
    /// 结束新手教学
    /// </summary>
    public void EndTeaching()
    {
        isTeaching = false;
        GameSet.instance.userData.IsFinishTeach = true;
        GameSet.instance.SaveUserData();
        OnEndTeaching?.Invoke();
    }

    public void ShowGameTip(string tip)
    {
        OnShowGameTip?.Invoke(tip);
    }

    /// <summary>
    /// 获取提示
    /// </summary>
    /// <returns></returns>
    public string GetTip()
    {
        for (var i = 0; i < nowLevelData.tipInfos.Count; i++)
        {
            if (_stepIndex.Contains(i)) continue;
            var key = $"Level{nowLevelData.levelID}Tips{i}";
            var tip = LocalizationManager.GetTermTranslation(key);
            switch (key) //单独记录提示进度
            {
            }
            return tip;
        }
        return "";
    }

    public void SetBtnVisible()
    {
        foreach (var btn in allVisibleBtns)
        {
            btn.SetBtnShow();
        }
    }
    
    #region Game
    
    public void StartGame(int levelID)
    {
        if (levelID <= 0 || levelID > GetTotalLevelCount()) return;
        InitGame(levelID);
        mainPageUI.HideUI();
        gameOverUI.HideUI();
        _stepIndex.Clear();
        ShowLoading(true,() =>
        {
            gameUI.ShowUI();
            GameSet.instance.gameManager.isGameStart = true;
            StartGameTimer();
            GameSet.instance.LevelStart(levelID);
        });
        //加载关卡UI
        if(nowLevel) Destroy(nowLevel.gameObject);
        var resName = $"Level{levelID}";
        Level prefab = ResourceManager.Instance.LoadPrefabSync<Level>(resName);
        nowLevel = Instantiate(prefab);
        nowLevel.transform.SetParent(content, false);
        CopyLocalTransform(prefab.transform, nowLevel.transform);
        nowLevel.transform.SetSiblingIndex(0);
    }

    private static void CopyLocalTransform(Transform source, Transform target)
    {
        target.localPosition = source.localPosition;
        target.localRotation = source.localRotation;
        target.localScale = source.localScale;

        if (source is RectTransform sourceRect && target is RectTransform targetRect)
        {
            targetRect.anchorMin = sourceRect.anchorMin;
            targetRect.anchorMax = sourceRect.anchorMax;
            targetRect.pivot = sourceRect.pivot;
            targetRect.sizeDelta = sourceRect.sizeDelta;
            targetRect.anchoredPosition = sourceRect.anchoredPosition;
        }
    }

    public void StartGameTimer()
    {
        if(startTimer) return;
        startTimer = true;
    }

    public void StopGameTimer(bool isFinishGame)
    {
        startTimer = false;
        if(isFinishGame) isGameStart = false;
    }
    
    private void InitGame(int levelID)
    {
        curScore = 0;
        _ruleLevelTables = null;
        if (TryLoadRuleLevel(levelID, out RuleLevelTables ruleLevelTables))
        {
            _ruleLevelTables = ruleLevelTables;
        }
        nowLevelData = GameSet.instance.matter.levels.Find(x => x.levelID == levelID);
        GameSet.instance.gameManager.enablePlayStepAudio = true;
    }

    public void AddScore()
    {
        curScore++;
        if(enablePlayStepAudio) GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.stepAudio);
        OnScoreChange?.Invoke();
        if (curScore < nowLevelData.targetScore) return;
        ShowGameMask();
        OnWinCheck?.Invoke();
    }

    /// <summary>
    /// 完成某个步骤
    /// </summary>
    /// <param name="stepIndex"></param>
    public void FinishStep(int stepIndex)
    {
        _stepIndex.Add(stepIndex);
    }

    /// <summary>
    /// 处理关卡结果
    /// </summary>
    /// <param name="isWin"></param>
    /// <param name="winStarsCount"></param>
    public void DelGameResult(bool isWin,int winStarsCount = 0)
    {
        if(!isGameStart) return;
        StopGameTimer(true);
        GameSet.instance.LevelEnd(nowLevelData.levelID,isWin? BXSdk.StageResult.StageSucc: BXSdk.StageResult.StageFail);
        if (isWin)
        {
            winStarNum = winStarsCount;
            SaveLevelFinishInfo(nowLevelData.levelID, winStarsCount);
            SaveLevelFinishInfo(nowLevelData.levelID + 1, 0);
        }
        //展示评分引导
        if (nowLevelData.levelID == 1 && !GameSet.instance.userData.IsShowReview) GameSet.instance.gameManager.reviewUI.ShowUI();
        gameOverUI.ShowUI(isWin);
    }

    /// <summary>
    /// 保存关卡完成信息
    /// </summary>
    /// <param name="levelID"></param>
    /// <param name="starsCount"></param>
    public void SaveLevelFinishInfo(int levelID, int starsCount)
    {
        var levelInfo = GameSet.instance.userData.LevelFinish.Find(x => x.levelID == levelID);
        if (levelInfo == null)
        {
            GameSet.instance.userData.LevelFinish.Add(new LevelFinishInfo{levelID = levelID,starsCount = starsCount});
            GameSet.instance.userData.Stars += starsCount;
        }
        else
        {
            if (levelInfo.starsCount < starsCount)
            {
                GameSet.instance.userData.Stars += starsCount - levelInfo.starsCount;
                levelInfo.starsCount = starsCount;
            }
        }
        GameSet.instance.SaveUserData();
    }

    /// <summary>
    /// 获取关卡获得的星星数
    /// </summary>
    /// <param name="levelID"></param>
    public int GetLevelStars(int levelID)
    {
        var levelInfo = GameSet.instance.userData.LevelFinish.Find(x => x.levelID == levelID);
        if (levelInfo != null) return levelInfo.starsCount;
        Debug.LogWarning("Level Stars Not Find !");
        return 0;
    }

    public void ShowGameMask()
    {
        OnShowGameMask?.Invoke();
    }

    public int GetTotalLevelCount()
    {
        int legacyCount = GameSet.instance.matter.levels.Count;
        try
        {
            int ruleCount = LubanConfig.LoadTables().TbLevelRuleStep.DataList.Select(x => x.LevelId).Distinct().Count();
            return Mathf.Max(legacyCount, ruleCount);
        }
        catch
        {
            return legacyCount;
        }
    }

    public bool TryLoadRuleLevel(int levelID, out RuleLevelTables ruleLevelTables)
    {
        ruleLevelTables = null;
        try
        {
            ruleLevelTables = RuleLevelConfigLoader.Load(levelID);
            return ruleLevelTables != null;
        }
        catch
        {
            return false;
        }
    }

    public LevelData GetLevelData(int levelID)
    {
        return GameSet.instance.matter.levels.Find(x => x.levelID == levelID);
    }
    
    #endregion
}
