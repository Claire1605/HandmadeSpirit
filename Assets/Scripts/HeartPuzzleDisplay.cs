using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartPuzzleDisplay : MonoBehaviour
{
    public TMPro.TextMeshProUGUI promptText;
    public TMPro.TextMeshProUGUI solvedText;


    public void ShowPrompt()
    {
        promptText.gameObject.SetActive(true);
        solvedText.gameObject.SetActive(false);

        StartCoroutine(ShowPromptCo());
    }

    public void ShowSolved()
    {
        promptText.gameObject.SetActive(false);
        solvedText.gameObject.SetActive(true);

        StartCoroutine(ShowSolvedCo());
    }

    private IEnumerator ShowPromptCo()
    {
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 1.0f * Time.deltaTime;
            promptText.alpha = alpha;
            yield return null;
        }
    }

    private IEnumerator ShowSolvedCo()
    {
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 1.0f * Time.deltaTime;
            solvedText.alpha = alpha;
            yield return null;
        }
    }
}
