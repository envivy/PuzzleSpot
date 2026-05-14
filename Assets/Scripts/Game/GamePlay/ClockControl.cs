using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ClockControl : MonoBehaviour
{
    [Header("Clock Hands")]
    public Transform hourHand;
    public Transform minuteHand;
    
    [Header("Rotation Settings")]
    [Tooltip("分针旋转速度（度/秒），负值表示逆时针")]
    public float minuteHandSpeed = -6f; // 默认逆时针旋转
    
    [Tooltip("时针旋转速度（度/小时），负值表示逆时针")]
    public float hourHandSpeed = -30f; // 默认逆时针旋转
    
    [Tooltip("时针与分针速度比例（12:1）")]
    public bool useRealisticRatio = true;
    
    private float currentMinuteRotation = 0f;
    private float currentHourRotation = 0f;
    
    private void Start()
    {
        // 初始化指针位置为当前时间
        System.DateTime currentTime = System.DateTime.Now;
        
        // 计算初始旋转角度（负值表示逆时针方向）
        currentMinuteRotation = -(currentTime.Minute * 6f + currentTime.Second * 0.1f);
        currentHourRotation = -(currentTime.Hour * 30f + currentTime.Minute * 0.5f);
        
        ApplyRotations();
    }
    
    private void Update()
    {
        // 旋转分钟指针
        currentMinuteRotation += minuteHandSpeed * Time.deltaTime;
        
        // 旋转时钟指针
        if(useRealisticRatio)
        {
            // 真实比例：时针速度是分针的1/12
            currentHourRotation += (minuteHandSpeed / 12f) * Time.deltaTime;
        }
        else
        {
            // 使用独立设置的小时速度（转换为度/秒）
            currentHourRotation += (hourHandSpeed / 3600f) * Time.deltaTime;
        }
        
        // 标准化角度到0-360度范围
        currentMinuteRotation = NormalizeAngle(currentMinuteRotation);
        currentHourRotation = NormalizeAngle(currentHourRotation);
        
        ApplyRotations();
    }
    
    private void ApplyRotations()
    {
        // 应用旋转（Z轴旋转）
        minuteHand.localRotation = Quaternion.Euler(0f, 0f, currentMinuteRotation);
        hourHand.localRotation = Quaternion.Euler(0f, 0f, currentHourRotation);
    }
    
    private float NormalizeAngle(float angle)
    {
        // 将角度标准化到0-360度范围
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }
    
    // 公开方法用于动态调整速度
    public void SetSpeeds(float newMinuteSpeed, float newHourSpeed = float.MinValue)
    {
        minuteHandSpeed = newMinuteSpeed;
        if(newHourSpeed != float.MinValue)
        {
            hourHandSpeed = newHourSpeed;
        }
    }
}
