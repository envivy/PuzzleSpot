using UnityEngine;
using UnityEngine.UI;

public class BtnVisibleController : MonoBehaviour
{
    private Button _btn;
    private UI_anim_Bt _anim;
    
    
    private void Awake()
    {
        _btn = GetComponent<Button>();
        _anim = GetComponent<UI_anim_Bt>();
    }

    private void Update()
    {
        _btn.enabled = !GameSet.instance.gameManager.isTeaching;
        _anim.enabled = !GameSet.instance.gameManager.isTeaching;
    }

    public void SetBtnShow()
    {
        gameObject.SetActive(GameSet.instance.EnableButtonShow);
    }
}
