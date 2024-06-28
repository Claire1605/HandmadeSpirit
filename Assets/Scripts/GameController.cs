using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public TextAsset questionsCSV;
    public Transform displayRoot;
    public Transform welcomeDisplayPrefab;
    public QuestionDisplay questionDisplayPrefab;

    private Transform welcomeDisplay;
    private List<Question> questions;
    private QuestionDisplay questionDisplay;

    private enum State
    {
        ShowingWelcome,
        ShowingQuestion,
        ShowingAnswer,
        ShowingResult,
    }

    private int curQuestion = -1;
    private State curState = State.ShowingWelcome;
    private int questionAnswer = -1;

    private float nextTransition = 5.0f;

    private void Start()
    {
        questions = QuestionHelper.GetQuestionsFromCSV(questionsCSV);
        foreach (var q in questions)
        {
            Debug.Log(q.ToString());
        }

        welcomeDisplay = Instantiate(welcomeDisplayPrefab, displayRoot);
        welcomeDisplay.gameObject.SetActive(true);

        questionDisplay = Instantiate(questionDisplayPrefab, displayRoot);
        questionDisplay.gameObject.SetActive(false);
    }

    private void Update()
    {
        bool forceTransition = false;
        if (curState == State.ShowingQuestion)
        {
            // Left Arrow
            if (UserInput.Instance.NavigationInput.x < 0)
            {
                questionAnswer = 0;
            }
            // Up Arrow
            else if (UserInput.Instance.NavigationInput.y > 0)
            {
                questionAnswer = 1;
            }
            // Right Arrow
            else if (UserInput.Instance.NavigationInput.x > 0)
            {
                questionAnswer = 2;
            }

            if (questionAnswer != -1)
            {
                forceTransition = true;
            }
        }

        if (forceTransition || nextTransition <= Time.time)
        {
            DoTransition();
        }
    }

    private void DoTransition()
    {
        switch (curState)
        {
            case State.ShowingWelcome:
            {
                welcomeDisplay.gameObject.SetActive(false);
                questionDisplay.gameObject.SetActive(true);

                curState = State.ShowingQuestion;
                questionAnswer = -1;
                ++curQuestion;
                questionDisplay.ShowQuestion(questions[curQuestion]);
                nextTransition = float.MaxValue;
            }
            break;

            case State.ShowingQuestion:
            {
                curState = State.ShowingAnswer;
                questionDisplay.ShowAnswer(questionAnswer);
                nextTransition = Time.time + 6;
            }
            break;

            case State.ShowingAnswer:
            {
                if (curQuestion < questions.Count - 1)
                {
                    curState = State.ShowingQuestion;
                    questionAnswer = -1;
                    ++curQuestion;
                    questionDisplay.ShowQuestion(questions[curQuestion]);
                    nextTransition = float.MaxValue;
                }
                else
                {
                    curState = State.ShowingResult;
                    questionDisplay.gameObject.SetActive(false);
                    nextTransition = float.MaxValue;
                }
            }
            break;

            case State.ShowingResult:
            {
                // Do nothing...
            }
            break;
        }
    }
}
