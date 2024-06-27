using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public TextAsset questionsCSV;

    private List<Question> questions;

    // Start is called before the first frame update
    void Start()
    {
        questions = QuestionHelper.GetQuestionsFromCSV(questionsCSV);
        foreach (var q in questions)
        {
            Debug.Log(q.ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
