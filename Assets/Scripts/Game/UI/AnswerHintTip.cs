using System.Text;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class AnswerHintTip : MonoBehaviour
{
    public Text tipContent;

    private void OnEnable()
    {
        SetTipContent();
    }

    private void SetTipContent()
    {
        tipContent.text = "";
        var levelData = GameSet.instance.gameManager.nowLevelData;
        var sb = new StringBuilder();
        for (var i = 0; i < levelData.tipInfos.Count; i++)
        {
            var key = $"Level{levelData.levelID}Tips{i}";
            var tip = $"{i + 1}.{LocalizationManager.GetTermTranslation(key)}";
            switch (key) //单独记录提示进度
            {
            }
            sb.AppendLine(tip);
        }
        tipContent.text = sb.ToString();
    }
}
