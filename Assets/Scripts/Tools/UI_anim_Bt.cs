using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UI_anim_Bt : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float OverTime = 0.25f;

    private Animator anim;

    void Start()
    {
        anim = this.GetComponent<Animator>();

    }
        

    //点击事件
    public void OnPointerDown(PointerEventData eventData)
    {
        GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.BtnAudio);
        anim.SetTrigger("Pressed");
    }

    //松开事件
    public void OnPointerUp(PointerEventData eventData)
    {
        anim.SetTrigger("Normal");
    }
}