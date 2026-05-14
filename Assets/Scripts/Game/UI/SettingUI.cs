using System;
using System.Collections.Generic;
using DG.Tweening;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : UI
{
    public Transform audioOnNode, audioOffNode;
    public Transform musicOnNode, musicOffNode;
    public SetLanguage english, arabic,portuguese,spanish;
    public Text curLanguage;
    
    public GameObject languageUI;
    public CanvasGroup languageUIGroup;
    public GameObject[] allLanguagesSelect;
    
    public override void ShowUI()
    {
        base.ShowUI();
    }

    private void OnEnable()
    {
        SetUIState();
        ShowLanguageText();
    }

    private void SetUIState() {
        audioOnNode.gameObject.SetActive(false);
        audioOffNode.gameObject.SetActive(false);
        musicOnNode.gameObject.SetActive(false);
        musicOffNode.gameObject.SetActive(false);
        if (GameSet.instance.userData.setData.audio)
        {
            audioOnNode.gameObject.SetActive(true);
        }
        else {
            audioOffNode.gameObject.SetActive(true);
        }
        if (GameSet.instance.userData.setData.music)
        {
            musicOnNode.gameObject.SetActive(true);
        }
        else
        {
            musicOffNode.gameObject.SetActive(true);
        }
    }

    private void ShowLanguageText()
    {
        switch (LocalizationManager.CurrentLanguage)
        {
            case "English":
                curLanguage.text = "English";
                break;
            case "Arabic":
                curLanguage.text = "عربي";
                break;
            case "Portuguese":
                curLanguage.text = "Português";
                break;
            case "Spanish":
                curLanguage.text = "Español";
                break;
        }
    }
    
    public void ShowLanguage()
    {
        RefreshLanguageBtnShow();
        languageUIGroup.transform.localScale = Vector3.zero;
        languageUIGroup.alpha = 1;
        languageUI.gameObject.SetActive(true);
        languageUIGroup.transform.DOScale(1.1f, 0.4f).OnComplete(() =>
        {
            languageUIGroup.transform.DOScale(1, 0.1f);
        });
    }

    public void CloseLanguage()
    {
        languageUIGroup.DOFade(0, 0.3f).OnComplete(() =>
        {
            languageUI.gameObject.SetActive(false);
        });
    }

    public void ChangeAudioClick() {
        GameSet.instance.userData.setData.audio = !GameSet.instance.userData.setData.audio;
        SetUIState();
    }

    public void ChangeMusicClick()
    {
        GameSet.instance.userData.setData.music = !GameSet.instance.userData.setData.music;
        SetUIState();
        GameSet.instance.gameManager.SetBGMState();
    }

    public void OnClickPrivacyBtn()
    {
        GameSet.instance.OpenPolicyPop();
    }
    
    public void OnClickEnglishBtn()
    {
        english.ApplyLanguage();
        RefreshLanguageBtnShow();
    }

    public void OnClickArabicBtn()
    {
        arabic.ApplyLanguage();
        RefreshLanguageBtnShow();
    }
    
    public void OnClickPortugueseBtn()
    {
        portuguese.ApplyLanguage();
        RefreshLanguageBtnShow();
    }
    
    public void OnClickSpanishBtn()
    {
        spanish.ApplyLanguage();
        RefreshLanguageBtnShow();
    }

    private void RefreshLanguageBtnShow()
    {
        foreach (var select in allLanguagesSelect)
        {
            select.SetActive(false);
        }

        switch (LocalizationManager.CurrentLanguage)
        {
            case "English":
                allLanguagesSelect[0].SetActive(true);
                break;
            case "Arabic":
                allLanguagesSelect[1].SetActive(true);
                break;
            case "Portuguese":
                allLanguagesSelect[2].SetActive(true);
                break;
            case "Spanish":
                allLanguagesSelect[3].SetActive(true);
                break;
        }
        ShowLanguageText();
    }
}
