using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Matter : ScriptableObject
{
    public bool isDebug;
    public AudioClip CloseBtnAudio;//关闭声音
    public AudioClip BtnAudio;//按钮声音
    public AudioClip ToastAudio;//toast声音
    public AudioClip mainPageAudio;
    public AudioClip winAudio;
    public AudioClip starAudio;
    public AudioClip stepAudio;
    
    public ToastUI toastUI;
    public Transform FlyCash;
    
    public List<LevelData>  levels;
    
    public void SetURL()
    {
        //HitOnEff_Url = AssetDatabase.GetAssetPath(HitOnEff);
    }

}
