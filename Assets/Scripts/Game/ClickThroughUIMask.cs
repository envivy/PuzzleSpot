using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickThroughUIMask : MonoBehaviour
{
    [Header("Hole Settings")]
    public Vector2 holeCenter = new Vector2(0.28f, 0.72f); // 标准化坐标[0-1]
    public Vector2 holeSize = new Vector2(0.45f, 0.25f);   // 标准化尺寸
    
    private Material maskMaterial;
    private RectTransform holeClickArea;

    void Start()
    {
        // 获取或创建材质实例
        Image image = GetComponent<Image>();
        maskMaterial = new Material(Shader.Find("Custom/UIMask"));
        image.material = maskMaterial;
        
        // 初始化Shader参数
        UpdateShaderProperties();
        
        // 创建透明点击区域
        CreateClickThroughArea();
    }

    void UpdateShaderProperties()
    {
        if (maskMaterial != null)
        {
            // 将标准化坐标转换为你的Shader需要的格式
            // 假设你的Shader使用1250表示中心，2500表示全尺寸
            maskMaterial.SetVector("_Center", new Vector4(
                holeCenter.x * 2500 - 1250, 
                holeCenter.y * 2500 - 1250, 
                0, 0));
                
            maskMaterial.SetVector("_HoleSize", new Vector4(
                holeSize.x * 2500, 
                holeSize.y * 2500, 
                0, 0));
        }
    }

    void CreateClickThroughArea()
    {
        // 创建透明点击区域
        GameObject clickArea = new GameObject("HoleClickArea");
        clickArea.transform.SetParent(transform, false);
        
        // 设置RectTransform
        holeClickArea = clickArea.AddComponent<RectTransform>();
        holeClickArea.anchorMin = Vector2.zero;
        holeClickArea.anchorMax = Vector2.zero;
        holeClickArea.pivot = new Vector2(0.5f, 0.5f);
        
        // 计算实际像素大小
        RectTransform parentRT = GetComponent<RectTransform>();
        holeClickArea.sizeDelta = new Vector2(
            holeSize.x * parentRT.rect.width,
            holeSize.y * parentRT.rect.height);
            
        holeClickArea.anchoredPosition = new Vector2(
            (holeCenter.x) * parentRT.rect.width,
            (holeCenter.y) * parentRT.rect.height);
        
        // 添加透明Image和Button
        Image img = clickArea.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.01f); // 几乎透明但可点击
        
        Button btn = clickArea.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(OnHoleClicked);
    }

    void OnHoleClicked()
    {
        // 获取当前指针位置
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // 存储所有被射线击中的结果
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // 过滤掉当前对象和它的子对象
        var validResults = results.Where(r => 
            r.gameObject != gameObject && 
            !r.gameObject.transform.IsChildOf(transform)
        ).ToList();

        // 执行穿透点击
        if (validResults.Count > 0)
        {
            // 找到最上层符合条件的对象
            GameObject target = validResults[0].gameObject;
        
            // 执行点击事件
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerClickHandler);
        
            Debug.Log($"点击穿透到: {target.name}");
        }
        else
        {
            Debug.Log("没有找到可穿透的UI对象");
        }
    }

    // 在编辑器修改属性时更新
    void OnValidate()
    {
        if (maskMaterial != null)
        {
            UpdateShaderProperties();
            
            if (holeClickArea != null && GetComponent<RectTransform>() != null)
            {
                RectTransform parentRT = GetComponent<RectTransform>();
                holeClickArea.sizeDelta = new Vector2(
                    holeSize.x * parentRT.rect.width,
                    holeSize.y * parentRT.rect.height);
                    
                holeClickArea.anchoredPosition = new Vector2(
                    (holeCenter.x - 0.5f) * parentRT.rect.width,
                    (holeCenter.y - 0.5f) * parentRT.rect.height);
            }
        }
    }
}
