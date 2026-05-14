using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickDetector :  MonoBehaviour
{
    #if UNITY_EDITOR
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 检测左键点击
        {
            // 检查是否点击到了 UI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // 获取当前鼠标下的 UI 对象
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = Input.mousePosition;

                // 存储所有被射线检测到的 UI 对象
                System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                if (results.Count > 0)
                {
                    GameObject clickedUI = results[results.Count-1].gameObject;
                    //Debug.LogWarning("点击的 UI 对象: " + clickedUI.name);
                }
            }
        }
    }
    #endif
}
