using System.Collections;
using UnityEngine;

public class CyclePoint : MonoBehaviour
{
    public GameObject[] objectsToCycle; // 在Inspector中分配的3个对象
    public float displayTime = 0.1f; // 每个对象显示的时间
    
    private int currentIndex = 0;

    void OnEnable()
    {
        currentIndex = 0;
        // 初始隐藏所有对象
        foreach (GameObject obj in objectsToCycle)
        {
            obj.SetActive(false);
        }
        
        // 开始循环
        StartCoroutine(CycleObjects());
    }
    
    IEnumerator CycleObjects()
    {
        while (true)
        {
            if (currentIndex == 3)
            {
                // 隐藏所有对象
                foreach (GameObject obj in objectsToCycle)
                {
                    obj.SetActive(false);
                }
            }
            else
            {
                // 显示当前对象
                objectsToCycle[currentIndex].SetActive(true);
            }
            
            // 等待指定时间
            yield return new WaitForSeconds(displayTime);
            
            // 移动到下一个对象
            currentIndex = (currentIndex + 1) % (objectsToCycle.Length + 1);
        }
    }
}
