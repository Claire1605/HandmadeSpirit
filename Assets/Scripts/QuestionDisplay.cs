using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionDisplay : MonoBehaviour
{
    public TMPro.TextMeshProUGUI questionText;
    public TMPro.TextMeshProUGUI answer0Text;
    public TMPro.TextMeshProUGUI answer1Text;
    public TMPro.TextMeshProUGUI answer2Text;


    public void Setup(Question question)
    {
        questionText.text = question.questionText;
        answer0Text.text = question.answer0;
        answer1Text.text = question.answer1;
        answer2Text.text = question.answer2;
    }
}
