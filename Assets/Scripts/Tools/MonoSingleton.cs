using UnityEngine;

/// <summary>
/// 动态(Dynamic)
/// </summary>
public abstract class D_MonoSingleton<T> : MonoBehaviour where T : D_MonoSingleton<T> 
{
	private static T _instance = null;

	public static T Instance
	{
        get {
            if (_instance == null)
            {
                GameObject go = new GameObject();
                DontDestroyOnLoad(go);
                go.name = "MonoSingleton:" + typeof(T).ToString();
                go.transform.localPosition = Vector3.zero;
                go.transform.localEulerAngles = Vector3.zero;
                go.transform.localScale = Vector3.one;
                _instance = go.AddComponent<T>();
            }
            return _instance;
        }
	}
    private void Awake()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {

    }

    private void OnDestroy()
    {
        _instance = null;

        Dispose();
    }

    protected virtual void Dispose()
    {

    }
}
	
/// <summary>
/// 静态(static)
/// </summary>
public abstract class S_MonoSingleton<T> : MonoBehaviour where T : S_MonoSingleton<T> 
{
	private static T _instance = null;
	public static T Instance
    {
        get
        {
            return _instance;
        }
    }

	private void Awake()
	{
		if (_instance != null && _instance != (T)this)
        {
			Destroy (gameObject);
			return;
		}
		_instance = (T)this;
		Initialize ();
	}

	protected virtual void Initialize() 
	{

	}

    private void OnDestroy()
    {
        _instance = null;
    }
}

/// <summary>
/// 静态常驻
/// </summary>
public abstract class O_MonoSingleton<T> : MonoBehaviour where T : O_MonoSingleton<T>
{
    private static T _instance = null;
    public static T Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != (T)this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = (T)this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    protected virtual void Initialize()
    {

    }

    private void OnDestroy()
    {
        _instance = null;
    }
}