using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultDisplay : MonoBehaviour
{
    public TMPro.TextMeshProUGUI nameText;
    public Image image;
    
    public TMPro.TextMeshProUGUI restartText;

    public void ShowResult(SpiritSO spirit)
    {
        restartText.gameObject.SetActive(false);
        nameText.text = spirit.spiritName;
        image.sprite = spirit.image;
    }

    public void ShowRestartText()
    {
        restartText.gameObject.SetActive(true);
    }
}
