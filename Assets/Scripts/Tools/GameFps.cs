using UnityEngine;

public class GameFps : MonoBehaviour
{
    private float FIxeFps = 0.5f;
    private int Fps = 0;
    private float deteTimes = 0f;
    private const float recTime = 0.5f;

    private void OnGUI()
    {
        Color color = GUI.color;
        GUI.color = Color.black;
        int guiFont = GUI.skin.label.fontSize;
        GUI.skin.label.fontSize = 40;
        GUI.Label(new Rect(Screen.width - 60, 0, 200, 200), Fps.ToString());
        GUI.skin.label.fontSize = guiFont;
        GUI.color = color;
    }

    private void Update()
    {
        deteTimes++;
        if (Time.realtimeSinceStartup > FIxeFps)
        {
            Fps = (int)(deteTimes / recTime);
            deteTimes = 0;
            FIxeFps += recTime;
        }
    }
}