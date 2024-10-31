using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ImageHelper
{
    public static void SetAlpha(this Image image, float a)
    {
        a = Mathf.Clamp01(a);
        image.color = new(image.color.r, image.color.g, image.color.b, a);
    }
}

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

        StartCoroutine(RevealResultCo());
    }

    public void ShowRestartText()
    {
        restartText.gameObject.SetActive(true);
    }

    private IEnumerator RevealResultCo()
    {
        nameText.alpha = 0.0f;
        image.SetAlpha(0.0f);

        yield return new WaitForSeconds(1);

        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 1.0f * Time.deltaTime;
            nameText.alpha = alpha;
            image.SetAlpha(alpha);
            yield return null;
        }

        nameText.alpha = 1.0f;
        image.SetAlpha(1.0f);
    }
}
