using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public TextAsset questionsCSV;
    public Camera resultCamera;
    public Transform displayRoot;
    public Transform welcomeDisplayPrefab;
    public QuestionDisplay questionDisplayPrefab;
    public ResultDisplay resultDisplayPrefab;
    public SpiritSO[] spirits;
    public bool slowReveal = true;

    public AudioSource selectionAudio;
    public AudioSource resultAudio;

    private Transform welcomeDisplay;
    private List<Question> questions;
    private QuestionDisplay questionDisplay;
    private ResultDisplay resultDisplay;

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

    private int[] result = new int[QuestionHelper.NUMBER_OF_SPRITS];

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

        resultDisplay = Instantiate(resultDisplayPrefab, displayRoot);
        resultDisplay.gameObject.SetActive(false);

        for (int i = 0; i < QuestionHelper.NUMBER_OF_SPRITS; ++i)
        {
            result[i] = 0;
        }
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

            // If we have recieved an answer
            if (questionAnswer != -1)
            {
                forceTransition = true;
                UpdateResult();
                Debug.Log(string.Join(", ", result));
                selectionAudio.Play();
            }
        }

        if (forceTransition || nextTransition <= Time.time)
        {
            DoTransition();
        }
    }

    private void UpdateResult()
    {
        int[] answerResult = null;
        if (questionAnswer == 0)
        {
            answerResult = questions[curQuestion].answer0Result;
        }
        else if (questionAnswer == 1)
        {
            answerResult = questions[curQuestion].answer1Result;
        }
        else if (questionAnswer == 2)
        {
            answerResult = questions[curQuestion].answer2Result;
        }

        if (result == null)
            return;

        for (int i = 0; i < QuestionHelper.NUMBER_OF_SPRITS; ++i)
        {
            result[i] += answerResult[i];
        }

        //Update colour
        int highScore = -100;
        int leadSpirit = 0;
        for (int i = 0; i < 5; i++)
        {
            if (result[i] > highScore)
            {
                highScore = result[i];
                leadSpirit = i;
            }
        }

        Color c = spirits[leadSpirit].colour;
        float qProgress = (float)(curQuestion + 1) / questions.Count;

        questionDisplay.oldColour = resultCamera.backgroundColor;
        questionDisplay.newColour = new Color(c.r * qProgress, c.g * qProgress, c.b * qProgress, 1);
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
                    resultDisplay.gameObject.SetActive(true);

                    // This is not efficient... but it will work
                    var resultSpiritIndex = result.ToList().IndexOf(result.Max());
                    var spirit = spirits[resultSpiritIndex];

                    resultDisplay.ShowResult(spirit);
                    if (!slowReveal)
                    {
                        resultCamera.backgroundColor = spirit.colour;
                    }
                  
                    resultAudio.Play();

                    nextTransition = float.MaxValue;
                }
            }
            break;

            case State.ShowingResult:
            {

            }
            break;
        }
    }
}
