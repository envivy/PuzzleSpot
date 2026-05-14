using UnityEngine;
using UnityEngine.Audio;

public class VolumeManager : MonoBehaviour
{
    public AudioMixer soundMixer;
    private bool isHeadphone = false;

    void Start()
    {
        // 检测设备是否插入耳机（移动端需适配）
        CheckAudioOutput();
    }

    void CheckAudioOutput()
    {
        // 移动端可通过系统API或插件（如Unity的MobileNative）检测耳机状态
        // 此处简化逻辑，实际需根据平台实现
        isHeadphone = false; // 默认按外放处理

        // 外放时提高音量
        if (!isHeadphone)
        {
            soundMixer.SetFloat("SpeakerVolume", 20f); // 外放+20dB
        }
        else
        {
            soundMixer.SetFloat("SpeakerVolume", 0f); // 耳机恢复默认
        }
    }
}
