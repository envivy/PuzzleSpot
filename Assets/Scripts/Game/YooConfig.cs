using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;


[CreateAssetMenu]
public class YooConfig : ScriptableObject
{
	public string PackageName = "GameData";
	public string AndroidNetPath = "https://touka-artifacts.oss-cn-beijing.aliyuncs.com/WXResource/SuanLiDaDueJue/Android";
	public string IOSNetPath = "https://touka-artifacts.oss-cn-beijing.aliyuncs.com/WXResource/SuanLiDaDueJue/IOS";
	public string WebNetPath = "https://touka-artifacts.oss-cn-beijing.aliyuncs.com/WXResource/SuanLiDaDueJue/Web";
	public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;
}
