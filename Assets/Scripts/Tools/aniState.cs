using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aniState : MonoBehaviour
{
    public Action AniCall;//动画播放完回调
    public Animation ani;
    public Animator anitor;

    public void Call() {
        AniCall?.Invoke();
    }

}
