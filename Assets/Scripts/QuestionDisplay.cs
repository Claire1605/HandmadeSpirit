using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionDisplay : MonoBehaviour
{
    public TMPro.TextMeshProUGUI questionText;
    public TMPro.TextMeshProUGUI answer0Text;
    public TMPro.TextMeshProUGUI answer1Text;
    public TMPro.TextMeshProUGUI answer2Text;

    public Vector2 givenAnswerPosition = new(0, 0);

    public Color oldColour;
    public Color newColour;
    private Camera resultCamera;
    private GameController gameController;

    private TMPro.TextMeshProUGUI AnswerText
    {
        get {
            return answer switch
            {
                0 => answer0Text,
                1 => answer1Text,
                2 => answer2Text,
                _ => null,
            };
        }
    }

    private int answer = -1;
    public Vector2 answer0TextStartPos;
    public Vector2 answer1TextStartPos;
    public Vector2 answer2TextStartPos;

    public void Setup()
    {
        answer0TextStartPos = answer0Text.rectTransform.anchoredPosition;
        answer1TextStartPos = answer1Text.rectTransform.anchoredPosition;
        answer2TextStartPos = answer2Text.rectTransform.anchoredPosition;
        gameController = FindObjectOfType<GameController>();
        resultCamera = gameController.resultCamera; //Sorry about this code, just trying to get it to work quickly!
    }

    public void ShowQuestion(Question question)
    {
        answer0Text.rectTransform.anchoredPosition = answer0TextStartPos;
        answer1Text.rectTransform.anchoredPosition = answer1TextStartPos;
        answer2Text.rectTransform.anchoredPosition = answer2TextStartPos;

        questionText.text = question.questionText;
        answer0Text.text = question.answer0;
        answer1Text.text = question.answer1;
        answer2Text.text = question.answer2;

        questionText.gameObject.SetActive(false);
        answer0Text.gameObject.SetActive(false);
        answer1Text.gameObject.SetActive(false);
        answer2Text.gameObject.SetActive(false);

        StartCoroutine(ShowQuestionCo());
    }

    public void ShowAnswer(int answer)
    {
        this.answer = answer;

        StartCoroutine(ShowAnswerCo());
    }

    private IEnumerator ShowQuestionCo()
    {
        questionText.gameObject.SetActive(true);

        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 1.0f * Time.deltaTime;
            questionText.alpha = alpha;
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        answer0Text.gameObject.SetActive(true);
        alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 2.0f * Time.deltaTime;
            answer0Text.alpha = alpha;
            yield return null;
        }

        answer1Text.gameObject.SetActive(true);
        alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 2.0f * Time.deltaTime;
            answer1Text.alpha = alpha;
            yield return null;
        }

        answer2Text.gameObject.SetActive(true);
        alpha = 0f;
        while (alpha < 1f)
        {
            alpha += 2.0f * Time.deltaTime;
            answer2Text.alpha = alpha;
            yield return null;
        }
    }

    private IEnumerator ShowAnswerCo()
    {
        Vector2 startPos = AnswerText.rectTransform.anchoredPosition;
        bool slowReveal = gameController.slowReveal;


        float t = 0f;
        while (t < 1f)
        {
            t += 1.0f * Time.deltaTime;
            if (t > 1) t = 1;
            float alpha = 1 - t;

            questionText.alpha = alpha;
            if (answer != 0) answer0Text.alpha = alpha;
            if (answer != 1) answer1Text.alpha = alpha;
            if (answer != 2) answer2Text.alpha = alpha;

            AnswerText.rectTransform.anchoredPosition = Vector2.Lerp(startPos, givenAnswerPosition, t);

            if (slowReveal)
            {
                resultCamera.backgroundColor = Color.Lerp(oldColour, newColour, t);
            }

            yield return null;
        }
    }
}
