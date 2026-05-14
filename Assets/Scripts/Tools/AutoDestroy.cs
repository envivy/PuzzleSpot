using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float time;
    private void Awake()
    {
        ExtendFun.DelayDoSecond(this, time, () =>
        {
            Destroy(gameObject);
        });
    }
}
