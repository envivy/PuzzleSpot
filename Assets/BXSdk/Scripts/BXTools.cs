using UnityEngine;

namespace BX
{
    public class BXTools
    {
        private const string BXSdk_PREFIX = "BXSdk_";
        
        public static float GetPlayerPrefsFloat(string _key, float _dValue = 0.0f)
        {
            float value = PlayerPrefs.GetFloat(string.Format("{0}{1}", BXSdk_PREFIX, _key), _dValue);
            return value;
        }

        public static void SavePlayerPrefsFloat(string _key, float _nValue)
        {
            PlayerPrefs.SetFloat(string.Format("{0}{1}", BXSdk_PREFIX, _key), _nValue);
            PlayerPrefs.Save();
        }

        public static int GetPlayerPrefsInt(string _key, int _dValue = 0)
        {
            int value = PlayerPrefs.GetInt(string.Format("{0}{1}", BXSdk_PREFIX, _key), _dValue);
            return value;
        }

        public static void SavePlayerPrefsInt(string _key, int _nValue)
        {
            PlayerPrefs.SetInt(string.Format("{0}{1}", BXSdk_PREFIX, _key), _nValue);
            PlayerPrefs.Save();
        }

        public static string GetPlayerPrefsString(string _key, string _dValue = "")
        {
            string value = PlayerPrefs.GetString(string.Format("{0}{1}", BXSdk_PREFIX, _key), _dValue);
            return value;
        }

        public static void SavePlayerPrefsString(string _key, string _nValue)
        {
            PlayerPrefs.SetString(string.Format("{0}{1}", BXSdk_PREFIX, _key), _nValue);
            PlayerPrefs.Save();
        }

        public static bool IfFirstCheckPlayerPrefs(string _key)
        {
            var isFirstClick = PlayerPrefs.GetInt(_key, 0) == 0;
            PlayerPrefs.SetInt(_key, 1);
            PlayerPrefs.Save();
            return isFirstClick;
        }

        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey($"{BXSdk_PREFIX}{key}");
        }


        public static void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey($"{BXSdk_PREFIX}{key}");
        }

        public static bool IsChinese()
        {
            bool isZh = true;
            string languageStr = Application.systemLanguage.ToString();
            if (languageStr.CompareTo("ChineseSimplified") == 0
                || languageStr.CompareTo("ChineseTraditional") == 0
                || languageStr.CompareTo("Chinese") == 0)
            {
                isZh = true;
            }
            else
            {
                isZh = false;
            }

            return isZh;
        }

        public static string GetVersionCode()
        {
#if UNITY_ADNROID && !UNITY_EDITOR
            AndroidJavaClass contextCls = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = contextCls.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageMngr = context.Call<AndroidJavaObject>("getPackageManager");
            string packageName = context.Call<string>("getPackageName");
            AndroidJavaObject packageInfo =
                packageMngr.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
            return packageInfo.Get<int>("versionCode");
#else
            return "";
#endif

        }
    }   
}