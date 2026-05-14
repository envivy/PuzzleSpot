using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToastUI : UI
{
    public Text msg;
    public override void ShowUI(string data)
    {
        GameSet.instance.audioManager.PlayAudio(GameSet.instance.matter.ToastAudio);
        base.ShowUI(data);
        msg.text = data;

        ExtendFun.DelayDoSecond(this, 3, () =>
        {
            Destroy(gameObject);
        });
    }
}
