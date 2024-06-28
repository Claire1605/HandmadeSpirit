using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultDisplay : MonoBehaviour
{
    public TMPro.TextMeshProUGUI nameText;
    public Image image;

    public void ShowResult(SpiritSO spirit)
    {
        nameText.text = spirit.spiritName;
        image.sprite = spirit.image;
    }
}
