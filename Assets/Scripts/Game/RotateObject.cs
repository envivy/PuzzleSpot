using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 90f; // 旋转速度（度/秒）

    private bool _enableRotate;

    public void StartRotate()
    {
        _enableRotate = true;
    }

    public void StopRotate()
    {
        _enableRotate = false;
    }
    
    void Update()
    {
        if(!_enableRotate) return;
        // 绕Y轴匀速旋转（世界坐标系）
        transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
    }
}
