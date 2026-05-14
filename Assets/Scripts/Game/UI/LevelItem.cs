using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class LevelItem : MonoBehaviour
{
    [HideInInspector] public int levelID;
    public Image levelImage;
    public Text levelText;
    public GameObject lockNode;
    public GameObject unlockNode;
    public GameObject finishImg;
    public GameObject[] allStars;
    private bool isLock;
    private LevelFinishInfo levelInfo;

    private void Start()
    {
        ShowLevelImage();
        UpdateState();
    }

    private void Update()
    {
        levelText.text = LocalizationManager.GetTermTranslation("LEVEL") + " " + levelID;
    }
    
    private void ShowLevelImage()
    {
        var levelData = GameSet.instance.gameManager.GetLevelData(levelID);
        if (levelData == null || string.IsNullOrEmpty(levelData.imagePath1)) return;
        ResourceManager.Instance.LoadResAsync<Sprite>(levelData.imagePath1, (sprite) =>
        {
            levelImage.sprite = sprite;
        });
    }

    public void UpdateState()
    {
        levelInfo =  GameSet.instance.userData.LevelFinish.Find(x => x.levelID == levelID);
        SetLockState();
        SetStarsState();
    }

    private void SetLockState()
    {
        isLock = levelInfo == null;
        lockNode.SetActive(isLock);
        unlockNode.SetActive(!isLock);
    }

    private void SetStarsState()
    {
        HideAllStars();
        if (levelInfo == null || levelInfo.starsCount == 0) return;
        finishImg.SetActive(levelInfo.starsCount == 3);
        for (var i = 0; i < levelInfo.starsCount; i++)
        {
            allStars[i].SetActive(true);
        }
    }

    private void HideAllStars()
    {
        finishImg.SetActive(false);
        foreach (var t in allStars)
        {
            t.SetActive(false);
        }
    }

    public void OnClickLevel()
    {
        GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.BtnAudio);
        if (isLock)
        {
            GameSet.instance.gameManager.PreUnlockLevel(levelID);
            return;
        }
        GameSet.instance.gameManager.StartGame(levelID);
    }
}
