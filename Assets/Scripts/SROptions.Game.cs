using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using SRDebugger;
using SRDebugger.Services;
using SRF;
using SRF.Service;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public partial class SROptions
{
    [Category("数值")]
    public int 关卡
    {
        get => GameSet.instance.gameManager.nowLevelData.levelID;
        set => GameSet.instance.gameManager.StartGame(value);
    }

    [Category("功能")]
    public bool UI开关
    {
        get => GameSet.instance.EnableButtonShow;
        set
        {
            GameSet.instance.EnableButtonShow = value;
            GameSet.instance.gameManager.SetBtnVisible();
        }
    }
}