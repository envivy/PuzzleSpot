using System.Collections;
using DG.Tweening;
using UnityEngine;

public class NetCheckManager : MonoBehaviour
{
    private bool _showToast;
    public CanvasGroup toastGroup;
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    void Update()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable) return;
        if (_showToast) return;
        StartCoroutine(ShowNoConnect());
        _showToast = true;
    }

    /// <summary>
    /// 断线提示
    /// </summary>
    private IEnumerator ShowNoConnect()
    {
        toastGroup.gameObject.SetActive(true);
        toastGroup.alpha = 1;
        yield return new WaitForSeconds(1f);
        toastGroup.DOFade(0, 1f).OnComplete(() => {toastGroup.gameObject.SetActive(false); });
    }
}
