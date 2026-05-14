using System;
using System.Collections.Generic;
using UnityEngine;

namespace BX
{
    public class BXSdk : BXSingleton<BXSdk>
    {
        public static string BXSdkVersion = "1.0.3";
        private bool isInit = false;
        private bool mIsNoAllAD = false;

        public bool IsNoAllAD
        {
            get => mIsNoAllAD;
            set
            {
                mIsNoAllAD = value;
                if (mIsNoAllAD)
                {
                    //TODO
                    HideBanner();
                    RemoveNative();
                }
            }
        }

        public void InitSdk(Action _initCallback = null)
        {
            Debug.Log("BXSdk call init sdk");
            Loom.QueueOnMainThread(obj => { }, "");
            if (isInit) return;
#if UNITY_EDITOR
            isInit = true;
            _initCallback?.Invoke();
            return;
#endif   
        }

        #region 广告
        public void RemoveNative()
        {
            Debug.Log("BXSdk call RemoveNative");
        }

        public bool IsReadyNative()
        {
            Debug.Log("BXSdk call IsReadyNative return false");
            return false;
        }

        public void ShowNative(RectTransform pRect, Camera pCam = null, string pAdPos = "")
        {
            Vector3[] tWorldCorners = new Vector3[4];
            pRect.GetWorldCorners(tWorldCorners);
            Vector2 tTopLeft = RectTransformUtility.WorldToScreenPoint(pCam, tWorldCorners[1]);
            Vector2 tBottomRight = RectTransformUtility.WorldToScreenPoint(pCam, tWorldCorners[3]);
            float tWidth = Mathf.Abs(tBottomRight.x - tTopLeft.x);
            float tHeight = Mathf.Abs(tBottomRight.y - tTopLeft.y);
            DataNativeAd data = new DataNativeAd();
            data.x = (int)tTopLeft.x;
            data.y = (int)(Screen.height - tTopLeft.y);
            data.width = (int)tWidth;
            data.height = (int)tHeight;
            Debug.Log($"BXSdk call ShowNative() " +
                      $"x: {(int)tTopLeft.x}  " +
                      $"y: {(int)(Screen.height - tTopLeft.y)}" +
                      $"width: {(int)tWidth}" +
                      $"height: {(int)tHeight}"
            );

        }

        public void ShowBanner(SDKBannerAlign _bannerAlign)
        {
            Debug.Log("BXSdk call ShowBanner:" + _bannerAlign);
        }

        public void HideBanner()
        {
            Debug.Log("BXSdk call HideBanner");
        }

        public void ShowInterstitial(string _adPos, Action _callback = null, IVADType _IvType = IVADType.IV1)
        {
            Debug.Log("BXSdk call ShowInterstitial _adPos:" + _adPos+" type:"+_IvType+" invoke close callback");
            _callback?.Invoke();
        }


        public void ShowReward(string _adPos, Action<bool> _rewardCallback = null, Action _showFailedCallback = null)
        {
            Debug.Log("BXSdk call ShowReward _adPos:" + _adPos + " invoke close success callback");
            _rewardCallback?.Invoke(true);
        }

        public bool IsReadyReward()
        {
            Debug.Log("BXSdk call IsReadyReward return true");
            return true;
        }

        public bool IsReadyInterstitial()
        {
            Debug.Log("BXSdk call IsReadyInterstitial return false");
            return true;
        }

        #endregion

        #region Event
        public void LogEvent(string _eventSort, Dictionary<string, object> _eventDic = null)
        {
            Debug.Log("BXSdk call LogEvent eventName:" + _eventSort+" eventDic:"+ DictionaryToString(_eventDic));

        }

        public static string DictionaryToString<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return "";
            var items = new List<string>();
            foreach (var kvp in dictionary)
            {
                items.Add($"{kvp.Key}:{kvp.Value}");
            }
            return $"{{{string.Join(", ", items)}}}";
        }
        #endregion

        #region 行为事件
        public void AdImpression(bool reward)
        {

        }

        public void PromoteSuccess(string network)
        {

        }

        public void LevelEnter(string _level)
        {
            Debug.Log("BXSdk call LevelEnter:" + _level);
        }

        public void LevelEnd(string _level, StageResult _stageResult)
        {
            Debug.Log("BXSdk call LevelEnd:" + _level + " result:" + _stageResult);
            if (_stageResult == StageResult.StageSucc)
            {
                
            }
            else
            {

            }
        }

        #endregion

        #region 按钮展示事件
        public void LogRewardAdBtnShow(string _pos)
        {
            Debug.Log("BXSdk call LogRewardAdBtnShow pos:" + _pos);
        }
        #endregion


        #region 在线参数
        public string GetConfigStr(string _key, string _defVal = "")
        {
            Debug.Log("BXSdk call GetConfigStr key:" + _key+" defaultValue:"+_defVal);
            return _defVal;
        }

        public int GetConfigInt(string _key, int _defVal = 0)
        {
            Debug.Log("BXSdk call GetConfigInt key:" + _key + " defaultValue:" + _defVal);
            return _defVal;
        }

        public bool GetConfigBool(string _key, bool _defVal = false)
        {
            Debug.Log("BXSdk call GetConfigBool key:" + _key + " defaultValue:" + _defVal);
            return _defVal;
        }

        #endregion

        #region Other
        public void OpenPrivacyURL(string _privacyURL)
        {
            Debug.Log("BXSdk call OpenPrivacyURL:" + _privacyURL);
            Application.OpenURL(_privacyURL);
        }

        public void Toast(string _text)
        {

         Debug.Log("BXSdk call Toast:" + _text);
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");

            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>(
                    "makeText",
                    context,
                    _text,
                    toastClass.GetStatic<int>("LENGTH_SHORT")
                );
                toast.Call("show");
            }));
        }
#else
            Debug.Log("BXSdk Toast is only supported on Android platform.");
#endif
        }

        public void Review()
        {
            
        }


        public void Shake()
        {
            Debug.Log("BXSdk call Shake");
            Handheld.Vibrate();
        }

        #endregion

        #region Enum
        public enum StageResult
        {
            StageSucc = 0,
            StageFail = 1,
            StageSkip = 2,
            StageBack = 3
        }

        public enum SDKBannerAlign : int
        {
            BannerCenterBottomAlign = 34,
            BannerCenterTopAlign = 10,
        }

        public enum AdsType
        {
            None,
            Banner,
            IV,
            RV,
            Native,
            Splash,
        }

        public enum IVADType
        {
            IV1 = 0,
            IV2 = 1,
            IV3 = 2,
            IV4 = 3,
            IV5 = 4,
            IV6 = 5,
            IV7 = 6,
            IV8 = 7,

            MAX
        }

        [Serializable]
        public class DataNativeAd
        {
            public int x;
            public int y;
            public int width;
            public int height;
            public string pos;
        }
        #endregion
    }
}