
using UnityEngine;
using System;
using System.Collections.Generic;
using I2.Loc;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using BX;
using UnityEngine.SceneManagement;
using YooAsset;

public class UserSkinData {
    public string ID;
    public bool isHave;
    public int adValue;
}


[Serializable]
public class LevelData
{
    public int levelID;
    public string title;
    public string tip;
    public int time;
    public int targetScore;
    public string imagePath1;
    public string imagePath2;
    public string imagePath3;
    public List<string> tipInfos;
}

[Serializable]
public class LevelFinishInfo
{
    public int levelID;
    public int starsCount;
}


//震动枚举
public enum AllShake
{
    btn,
    lit,
    tips
}
public enum SkinKind
{
    Cash,
    AD
}


//用户数据
public class UserData
{
    public SetData setData = new SetData();
    public int Coin = 0;//金币
    public int Level = 1;//关卡
    public int Stars = 0;//星星
    public int HintCount = 2;//提示次数
    public float VideoProgress = 0f;
    public bool isDefault = true;//是不是自然用户
    public bool IsFinishTeach = false;
    public bool IsShowReview = false;
    public string Language;
    public List<LevelFinishInfo> LevelFinish = new List<LevelFinishInfo>{ new LevelFinishInfo { levelID = 1, starsCount = 0 } }; //关卡信息
}

//设置
public class SetData
{
    public bool audio = true;
    public bool music = true;
}



public enum Lan {
    CN,
    EN
}


public class GameSet
{
    public static GameSet _instance;
    public static GameSet instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameSet();
            }
            return _instance;
        }
    }
    public bool isLoad = false;
    public AudioManager audioManager;
    public Matter matter;
    public GameManager gameManager;
    public UserData userData = new UserData();//玩家数据
    public Action ADDoneCall;
    public bool LevelFinish = false; //用于记录埋点上报

    public bool EnableButtonShow = true;

    public string GetMachineID()
    {
        return GetMD5String(SystemInfo.deviceUniqueIdentifier);
    }

    public string GetMD5String(string strText)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] encryptedBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(strText));
        StringBuilder data = new StringBuilder();
        for (int i = 0; i < encryptedBytes.Length; i++)
        {
            data.AppendFormat("{0:x2}", encryptedBytes[i]);
        }
        return data.ToString();
    }


    public Lan GetLan() {
        string msg = LocalizationManager.GetTermTranslation("Lang");
        Lan nowLan = Lan.CN;
        switch (msg)
        {
            case "CN":
                nowLan = Lan.CN;
                break;
            case "EN":
                nowLan = Lan.EN;
                break;
        }
        return nowLan;
    }


    //播放激励视频
    public void PlayVideoAD(string key,Action call,Action failCall = null)
    {
        if (matter.isDebug == false)
        {
            ADDoneCall = call;
            if (BXSdk.Instance.IsReadyReward())
            {
                BXSdk.Instance.ShowReward(key, (result) =>
                {
                    if (result)
                    {
                        userData.VideoProgress += 0.25f;
                        if (userData.VideoProgress > 1f) userData.VideoProgress = 1f;
                        SaveUserData();
                        gameManager.isShowVideoReward = true;
                        ADDoneCall?.Invoke();
                    }
                    else
                    {
                        failCall?.Invoke();
                    }
                });
            }
            else
            {
                gameManager.ShowToast(LocalizationManager.GetTermTranslation("ADisNotReady"));
            }
        }
        else {
            call?.Invoke();
        }
    }

    //播放插屏广告
    public void PlayInsertAD(string key)
    {
        if (matter.isDebug == false) {
            BXSdk.Instance.ShowInterstitial(key, () =>
            {
                Debug.Log("插屏关闭了");
            });
        }
    }
    
    public void ShowBanner() {
        BXSdk.Instance.ShowBanner(BXSdk.SDKBannerAlign.BannerCenterBottomAlign);
    }
    
    public void HideBanner() {
        BXSdk.Instance.HideBanner();
    }

    public void LevelStart(int lv) {
        BXSdk.Instance.LevelEnter(lv.ToString());
        LevelFinish = false;
    }

    public void LevelEnd(int level, BXSdk.StageResult state)
    {
        if(LevelFinish) return; //防止重复上报
        BXSdk.Instance.LevelEnd(level.ToString(), state);
        LevelFinish = true;
    }
    
    public void LogEvent(string a,string b,string c) {
        BXSdk.Instance.LogEvent(a, new Dictionary<string, object>() { 
            { b, c } 
        });
    }

    public void LogEvent(string a)
    {
        BXSdk.Instance.LogEvent(a);
    }
    
    public void ShowNative(RectTransform rc)
    {
        if (BXSdk.Instance.IsReadyNative()) {
            BXSdk.Instance.ShowNative(rc);
        }
    }

    public void RemoveNative()
    {
        BXSdk.Instance.RemoveNative();
    }

    public void OpenPolicyPop() {
        BXSdk.Instance.OpenPrivacyURL("https://kkvgame.com/privacy.html");
    }
    
    public void SetGameConfig()
    {
        if (isLoad)
        {
            return;
        }
        else
        {
            isLoad = true;

        }


        Application.targetFrameRate = 60;

        matter = Resources.Load<Matter>("config/Matter");

    

        if (matter.isDebug)
        {
            SRDebug.Init();
        }

        if (ES3.KeyExists("GameData"))
        {
            //老用户
            Debug.Log("拿取到了老用户数据");
            userData = ES3.Load("GameData") as UserData;
        }
        else
        {
            Debug.Log("新用户数据");
            userData = new UserData();

            userData.setData = new SetData();




        }

        CheckUserData();
        SaveUserData();

      


    }

    
    //效验本地数据
    public void CheckUserData() {

    }


    //保存玩家数据
    public void SaveUserData()
    {
        ES3.Save("GameData", userData);
    }

    
    //手机震动
    public void Shake(AllShake kind)
    {
        if (userData.setData.music)
        {
            switch (kind)
            {
                case AllShake.btn:
                    //按钮点击
                    break;
                case AllShake.lit:
                    break;
                case AllShake.tips:
                    break;
            }
        }
    }


    //List对象乱序
    public List<T> RandomSortList<T>(List<T> list)
    {
        var random = new System.Random();
        var newList = new List<T>();
        foreach (var item in list)
        {
            newList.Insert(random.Next(newList.Count), item);
        }
        return newList;
    }
    public float CheckAngle(float value)
    {
        float angle = value - 180;

        if (angle > 0)
            return angle - 180;

        return angle + 180;
    }

    //生成唯一ID
    public string GetID()
    {
        return Guid.NewGuid().ToString();
    }

    //获取当前时间戳
    public long GetNowTimeS()
    {
        TimeSpan mTimeSpan = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0);
        long time = (long)mTimeSpan.TotalSeconds;
        return time;
    }

    //传入秒数返回时间
    public string DealTime(long timeNum)
    {
        string timeStr = "";
        int h = Mathf.FloorToInt(timeNum / 3600);
        int m = Mathf.FloorToInt((timeNum % 3600) / 60);
        int s = (int)(timeNum % 60);

        if (h > 0)
        {
            timeStr = (h > 9 ? h.ToString() : ("0" + h)) + ":" + (m > 9 ? m.ToString() : ("0" + m)) + ":" + (s > 9 ? s.ToString() : ("0" + s));
        }
        else
        {
            timeStr = (m > 9 ? m.ToString() : ("0" + m)) + ":" + (s > 9 ? s.ToString() : ("0" + s));
        }


        return timeStr;
    }

	public static void DealData(byte[] pData)
	{
		int tIndex = 0;
		byte[] tCodes = new byte[] { 11, 2, 1, 5, 8, 50 };
		for (int i = 0; i < pData.Length; i += 500)
		{
			pData[i] = (byte)(pData[i] ^ tCodes[tIndex]);
			tIndex++;
			tIndex = tIndex % tCodes.Length;
		}
	}
}

public class TKResEncryption : IEncryptionServices
{
	public EncryptResult Encrypt(EncryptFileInfo pFileInfo)
	{
		byte[] tFileData = File.ReadAllBytes(pFileInfo.FilePath);
		GameSet.DealData(tFileData);

		EncryptResult result = new EncryptResult();
		result.Encrypted = true;
		result.EncryptedData = tFileData;
		return result;
	}
}

public class TKResDecryption : IDecryptionServices
{
	public AssetBundle LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
	{
		Debug.Log("LoadAssetBundle");
		byte[] encryptedData = File.ReadAllBytes(fileInfo.FileLoadPath);
		GameSet.DealData(encryptedData); // 解密数据，使用相同的 DealData 方法

		managedStream = new MemoryStream(encryptedData);
		return AssetBundle.LoadFromMemory(encryptedData);
	}

	public AssetBundleCreateRequest LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
	{
		Debug.Log("LoadAssetBundleAsync");
		byte[] encryptedData = File.ReadAllBytes(fileInfo.FileLoadPath);
		GameSet.DealData(encryptedData); // 解密数据，使用相同的 DealData 方法

		managedStream = new MemoryStream(encryptedData);
		return AssetBundle.LoadFromMemoryAsync(encryptedData);
	}
}
