using System;
using System.Collections;
using BX;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class InitUI : MonoBehaviour
{
	public Text DownLoadBarLab;
	public Image DownLoadBar;
	private const float LoadTime = 3f;
	public RectTransform layOut;
	public GameObject[] logo;

	private void Awake()
	{
		CheckLanguage();
		ResourceManager.Instance.Init(() =>
		{
			CheckAB();
			BXSdk.Instance.InitSdk();
		});
	}
	
	/// <summary>
	/// 检测系统语言
	/// </summary>
	private void CheckLanguage()
	{
		var language = PlayerPrefs.GetString("Language");
		if (!string.IsNullOrEmpty(language))
		{
			LocalizationManager.CurrentLanguage = language;
			return;
		}
		var lang = Application.systemLanguage;
		switch (lang)
		{
			case SystemLanguage.Arabic:
				LocalizationManager.CurrentLanguage = "Arabic";
				break;
			case SystemLanguage.Portuguese:
				LocalizationManager.CurrentLanguage = "Portuguese";
				break;
			case SystemLanguage.Spanish:
				LocalizationManager.CurrentLanguage = "Spanish";
				break;
			default:
				LocalizationManager.CurrentLanguage = "English";
				break;
		}
		PlayerPrefs.SetString("Language", LocalizationManager.CurrentLanguage);
	}
	
	private void Start()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(layOut);
		ShowLogo();
		StartCoroutine(LoadProgressBar());
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

	public void CheckAB() {
		StartCoroutine(ResourceManager.Instance.UpdatePackageVersion((bool state) =>
		{
			if (state)
			{
				StartCoroutine(ResourceManager.Instance.Download(
						(bool state3) =>
						{
							if (state3)
							{
#if UNITY_WEBGL
								LoadAllAsset(() =>
								{
									ResourceManager.Instance.LoadScene("Game");
								});
#else
								//ResourceManager.Instance.LoadScene("Game");
#endif

							}
							else
							{
								CheckAB();
							}
						},
						(float val) =>
						{
							SetDownloadState(val);

						}));
			}
			else
			{
				CheckAB();
			}
		}));

	}
	
	public void SetDownloadState(float v) {
		DownLoadBarLab.text = (v * 100).ToString("0");
		DownLoadBar.fillAmount = v;
	}
	
	private IEnumerator LoadProgressBar()
	{
		var elapsedTime = 0f;
		var startValue = 0f;
		var endValue = 1f;
		while (elapsedTime < LoadTime)
		{
			var progress = Mathf.Lerp(startValue, endValue, elapsedTime / LoadTime);
			DownLoadBar.fillAmount = progress;
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		DownLoadBar.fillAmount = endValue;
		ResourceManager.Instance.LoadScene("Game");
	}

	public void LoadAllAsset(Action loadDoneCall)
	{
		var allNeedLoad = ResourceManager.Instance.LoadAllAsset();
		int index = 0;
		foreach (var item in allNeedLoad)
		{
			string kind = item.AssetPath.Split('.')[1];
			switch (kind)
			{
				case "wav":
				case "mp3":
					ResourceManager.Instance.LoadResAsync(item.Address, (AudioClip want) =>
					{
						index++;
						SetDownloadState((float)index / (float)allNeedLoad.Length);
						if (index >= allNeedLoad.Length)
						{
							loadDoneCall?.Invoke();
						}
					});
					break;
				case "prefab":
					ResourceManager.Instance.LoadResAsync(item.Address, (GameObject want) =>
					{
						index++;
						SetDownloadState((float)index / (float)allNeedLoad.Length);
						if (index >= allNeedLoad.Length)
						{
							loadDoneCall?.Invoke();
						}
					});
					break;
				case "png":
					ResourceManager.Instance.LoadResAsync(item.Address, (Sprite want) =>
					{
						index++;
						SetDownloadState((float)index / (float)allNeedLoad.Length);
						if (index >= allNeedLoad.Length)
						{
							loadDoneCall?.Invoke();
						}
					});
					break;
				case "mat":
					ResourceManager.Instance.LoadResAsync(item.Address, (Material want) =>
					{
						index++;
						SetDownloadState((float)index / (float)allNeedLoad.Length);
						if (index >= allNeedLoad.Length)
						{
							loadDoneCall?.Invoke();
						}
					});
					break;
				default:
					index++;
					SetDownloadState((float)index / (float)allNeedLoad.Length);
					print("δ���������" + kind);
					break;
			}
		}

	}

}
