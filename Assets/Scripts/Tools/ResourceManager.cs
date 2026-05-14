using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using YooAsset;
using System.IO;


public class ResourceManager : D_MonoSingleton<ResourceManager>
{
	private ResourcePackage mMainPackage;

	private Dictionary<string, UObject> mResDic;

	static YooConfig yooConfig;

	Action IntSucCall;

	public AssetInfo[] LoadAllAsset()
	{

		return mMainPackage.GetAssetInfos("Def");
	}

	public void Init(Action IntSucCall)
	{
		this.IntSucCall = IntSucCall;

		yooConfig= Resources.Load<YooConfig>("YooConfig");


		YooAssets.Initialize();
		mMainPackage = YooAssets.CreatePackage(yooConfig.PackageName);
		YooAssets.SetDefaultPackage(mMainPackage);
		mResDic = new Dictionary<string, UObject>();
		YooAssets.SetCacheSystemDisableCacheOnWebGL();
		switch (yooConfig.PlayMode)
		{
			case EPlayMode.EditorSimulateMode:
				StartCoroutine(InitializeMainEditor());
				break;
			case EPlayMode.OfflinePlayMode:
				StartCoroutine(InitializeMainOffline());
				break;
			case EPlayMode.HostPlayMode:
				StartCoroutine(NetInitializeYooAsset());
				break;
			case EPlayMode.WebPlayMode:
				StartCoroutine(WebInitializeYooAsset());
				break;
		}

	}


	private IEnumerator InitializeMainEditor()
	{
		var initParameters = new EditorSimulateModeParameters();
		var simulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline, yooConfig.PackageName);
		initParameters.SimulateManifestFilePath = simulateManifestFilePath;
		yield return mMainPackage.InitializeAsync(initParameters);

		IntSucCall?.Invoke();
	}

	private IEnumerator InitializeMainOffline()
	{
		var initParameters = new OfflinePlayModeParameters();
		initParameters.DecryptionServices = new TKResDecryption();
		yield return mMainPackage.InitializeAsync(initParameters);
		IntSucCall?.Invoke();


		
	}

	//联机模式的代码

	private IEnumerator NetInitializeYooAsset()
	{
		var initParameters = new HostPlayModeParameters();
		initParameters.BuildinQueryServices = new GameQueryServices();
		initParameters.DecryptionServices = new TKResDecryption();
#if UNITY_ANDROID
		initParameters.RemoteServices = new RemoteServices(yooConfig.AndroidNetPath, yooConfig.AndroidNetPath);
#elif UNITY_IOS
		initParameters.RemoteServices = new RemoteServices(yooConfig.IOSNetPath, yooConfig.IOSNetPath);
#endif


		var initOperation = mMainPackage.InitializeAsync(initParameters);
		yield return initOperation;

		if (initOperation.Status == EOperationStatus.Succeed)
		{
			Debug.Log("资源包初始化成功！");
		}
		else
		{
			Debug.LogError($"资源包初始化失败：{initOperation.Error}");
		}

		IntSucCall?.Invoke();
	}


	//web模式初始化
	private IEnumerator WebInitializeYooAsset()
	{
		var initParameters = new WebPlayModeParameters();
		initParameters.BuildinQueryServices = new GameQueryServices();
		initParameters.RemoteServices = new RemoteServices(yooConfig.WebNetPath, yooConfig.WebNetPath);
		var initOperation = mMainPackage.InitializeAsync(initParameters);
		yield return initOperation;

		if (initOperation.Status == EOperationStatus.Succeed)
		{
			Debug.Log("资源包初始化成功！");
		}
		else
		{
			Debug.LogError($"资源包初始化失败：{initOperation.Error}");
		}

		IntSucCall?.Invoke();
	}

	/// <summary>
	/// 远端资源地址查询服务类
	/// </summary>
	private class RemoteServices : IRemoteServices
	{
		private readonly string _defaultHostServer;
		private readonly string _fallbackHostServer;

		public RemoteServices(string defaultHostServer, string fallbackHostServer)
		{
			_defaultHostServer = defaultHostServer;
			_fallbackHostServer = fallbackHostServer;
		}
		string IRemoteServices.GetRemoteMainURL(string fileName)
		{
			return $"{_defaultHostServer}/{fileName}";
		}
		string IRemoteServices.GetRemoteFallbackURL(string fileName)
		{
			return $"{_fallbackHostServer}/{fileName}";
		}
	}




	string packageVersion = "1.0.0";
	public IEnumerator UpdatePackageVersion(Action<bool> call)
	{
		//2.获取资源版本
		var operation = mMainPackage.UpdatePackageVersionAsync();
		yield return operation;
		if (operation.Status != EOperationStatus.Succeed)
		{
			Debug.LogError("版本号更新失败，可能是找不到服务器");
			call?.Invoke(false);
			yield break;
		}
		//这是获取到的版本号，在下一个步骤要用
		packageVersion = operation.PackageVersion;
		print("获取到了线上版本号：" + packageVersion);


		//3.获取补丁清单
		var op = mMainPackage.UpdatePackageManifestAsync(packageVersion);
		yield return op;
		if (op.Status != EOperationStatus.Succeed)
		{
			call?.Invoke(false);
			Debug.LogError("Mainfest更新失败！");
		}
		else
		{
			call?.Invoke(true);
		}

	}



	int downloadingMaxNum = 10;
	int failedTryAgain = 3;
	int timeout = 60;

	public IEnumerator Download(Action<bool> stateCall,Action<float>downloadCall)
	{
		var downloader = mMainPackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);
		//下载数量是0，直接就完成了
		if (downloader.TotalDownloadCount == 0)
		{
			Debug.Log("没有资源要下载");
			stateCall?.Invoke(true);
			yield break;
		}

		//注册一些回调
		downloader.OnDownloadErrorCallback += (string fileName, string error) =>
		{
			print("下载失败:" + fileName + "错误信息:" + error);
		};
		downloader.OnDownloadProgressCallback += (int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes) => {

			float val = ((float)currentDownloadBytes / (float)totalDownloadBytes);
			print("下载进度:" + val);
			downloadCall?.Invoke(val);
		};
		downloader.OnDownloadOverCallback += (bool suc) => {
			print("下载结束：" + suc);
		};
		downloader.OnStartDownloadFileCallback += (string fileName, long sizeBytes) => {
			print("开始下载：" + fileName + " 文件大小：" + sizeBytes);
		};

		//开始下载
		downloader.BeginDownload();
		//等待下载完成
		yield return downloader;
		//检查状态
		if (downloader.Status == EOperationStatus.Succeed)
		{
			Debug.Log("下载完成");
			stateCall?.Invoke(true);
		}
		else
		{
			Debug.Log("下载失败");
			stateCall?.Invoke(false);
		}
	}




	//联机模式结束








	/// <summary>
	/// 资源文件查询服务类
	/// </summary>
	public class GameQueryServices : IBuildinQueryServices
	{
		/// <summary>
		/// 查询内置文件的时候，是否比对文件哈希值
		/// </summary>
		public static bool CompareFileCRC = false;

		public bool Query(string packageName, string fileName, string fileCRC)
		{
			// 注意：fileName包含文件格式
			return StreamingAssetsHelper.FileExists(packageName, fileName, fileCRC);
		}
	}

#if UNITY_EDITOR
	public sealed class StreamingAssetsHelper
	{
		public static void Init() { }
		public static bool FileExists(string packageName, string fileName, string fileCRC)
		{
			string filePath = Path.Combine(Application.streamingAssetsPath, yooConfig.PackageName, packageName, fileName);
			if (File.Exists(filePath))
			{
				if (GameQueryServices.CompareFileCRC)
				{
					string crc32 = YooAsset.Editor.EditorTools.GetFileCRC32(filePath);
					return crc32 == fileCRC;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}
	}
#else
public sealed class StreamingAssetsHelper
{
    private class PackageQuery
    {
        public readonly Dictionary<string, BuildinFileManifest.Element> Elements = new Dictionary<string, BuildinFileManifest.Element>(1000);
    }

    private static bool _isInit = false;
    private static readonly Dictionary<string, PackageQuery> _packages = new Dictionary<string, PackageQuery>(10);

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        if (_isInit == false)
        {
            _isInit = true;

            var manifest = Resources.Load<BuildinFileManifest>("BuildinFileManifest");
            if (manifest != null)
            {
                foreach (var element in manifest.BuildinFiles)
                {
                    if (_packages.TryGetValue(element.PackageName, out PackageQuery package) == false)
                    {
                        package = new PackageQuery();
                        _packages.Add(element.PackageName, package);
                    }
                    package.Elements.Add(element.FileName, element);
                }
            }
        }
    }

    /// <summary>
    /// 内置文件查询方法
    /// </summary>
    public static bool FileExists(string packageName, string fileName, string fileCRC32)
    {
        if (_isInit == false)
            Init();

        if (_packages.TryGetValue(packageName, out PackageQuery package) == false)
            return false;

        if (package.Elements.TryGetValue(fileName, out var element) == false)
            return false;

        if (GameQueryServices.CompareFileCRC)
        {
            return element.FileCRC32 == fileCRC32;
        }
        else
        {
            return true;
        }
    }
}
#endif




	public T LoadResSync<T>(string pResName) where T : UObject
	{
		if (!mResDic.ContainsKey(pResName))
		{
			AssetHandle pHandler = mMainPackage.LoadAssetSync<T>(pResName);
			mResDic[pResName] = pHandler.AssetObject;
			pHandler.Release();
		}

		return mResDic[pResName] as T;
	}

	public T LoadPrefabSync<T>(string pResName) where T : Component
	{
		GameObject tGob = LoadResSync<GameObject>(pResName);
		if (tGob == null)
			return null;

		return tGob.GetComponent<T>();
	}

	public T LoadScriptableSync<T>(string pResName) where T : ScriptableObject
	{
		ScriptableObject tSob = LoadResSync<ScriptableObject>(pResName);
		if (tSob == null)
			return null;

		return tSob as T;
	}

	public void LoadResAsync<T>(string pResName, Action<T> pDelComplete) where T : UObject
	{
		string tResKey = pResName;
		if (!mResDic.ContainsKey(pResName))
		{
			mMainPackage.LoadAssetAsync<T>(pResName).Completed += (pHandler) =>
			{
				mResDic[pResName] = pHandler.AssetObject;
				pDelComplete?.Invoke(mResDic[pResName] as T);
				pHandler.Release();
			};
		}
		else
		{
			pDelComplete?.Invoke(mResDic[pResName] as T);
		}
	}

	public void LoadPrefabAsync<T>(string pResName, Action<T> pDelComplete) where T : Component
	{
		LoadResAsync<GameObject>(pResName, pResult =>
		{
			pDelComplete?.Invoke(pResult.GetComponent<T>());
		});
	}

	public void LoadScriptableAsync<T>(string pResName, Action<T> pDelComplete) where T : ScriptableObject
	{
		LoadResAsync<ScriptableObject>(pResName, pResult =>
		{
			pDelComplete?.Invoke(pResult == null ? null : pResult as T);
		});
	}

	public SceneHandle LoadScene(string pSceneName)
	{
		return mMainPackage.LoadSceneAsync(pSceneName);
	}

	public void Release()
	{
		mResDic.Clear();
		mMainPackage.UnloadUnusedAssets();
	}

}