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
    public HeartPuzzleDisplay heartPuzzleDisplayPrefab;
    public ResultDisplay resultDisplayPrefab;
    public FinalPuzzleDisplay finalPuzzleDisplayPrefab;
    public SpiritSO[] spirits;
    public bool slowReveal = true;
    public int whenIsTheHeartPuzzle = 5;
    public float finalPuzzleCompleteTime = 10;

    public bool DEBUG_startWithHeartPuzzle = false;
    public bool DEBUG_startWithFinalPuzzle = false;

    public AudioSource selectionAudio;
    public AudioSource heartAudio;
    public AudioSource finalPuzzleAudio;
    public AudioSource resultAudio;

    private Transform welcomeDisplay;
    private List<Question> questions;
    private QuestionDisplay questionDisplay;
    private HeartPuzzleDisplay heartPuzzleDisplay;
    private ResultDisplay resultDisplay;
    private FinalPuzzleDisplay finalPuzzleDisplay;

    private enum State
    {
        ShowingWelcome,
        ShowingQuestion,
        ShowingAnswer,
        ShowingHeartPuzzle,
        ShowingHeartPuzzleSolved,
        ShowingFinalPuzzle,
        ShowingResult,
    }

    private int curQuestion = 0;
    private State curState = State.ShowingWelcome;
    private int questionAnswer = -1;

    private float nextTransition = float.MaxValue;
    private float blockInputUntil = float.MinValue;

    private float finalPuzzleTime;

    private int[] result = new int[QuestionHelper.NUMBER_OF_SPRITS];

    private void Start()
    {
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }

        questions = QuestionHelper.GetQuestionsFromCSV(questionsCSV);
        foreach (var q in questions)
        {
            Debug.Log(q.ToString());
        }

        welcomeDisplay = Instantiate(welcomeDisplayPrefab, displayRoot);
        welcomeDisplay.gameObject.SetActive(true);

        questionDisplay = Instantiate(questionDisplayPrefab, displayRoot);
        questionDisplay.Setup();
        questionDisplay.gameObject.SetActive(false);

        heartPuzzleDisplay = Instantiate(heartPuzzleDisplayPrefab, displayRoot);
        heartPuzzleDisplay.gameObject.SetActive(false);

        finalPuzzleDisplay = Instantiate(finalPuzzleDisplayPrefab, displayRoot);
        finalPuzzleDisplay.gameObject.SetActive(false);

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
        if (blockInputUntil < Time.time)
        {
            if (curState == State.ShowingWelcome)
            {
                if (UserInput.Instance.AnyAnswerTriggered)
                {
                    forceTransition = true;
                }
            }
            else if (curState == State.ShowingQuestion)
            {
                if (UserInput.Instance.AnswerLeftTriggered)
                {
                    questionAnswer = 0;
                }
                else if (UserInput.Instance.AnswerCenterTriggered)
                {
                    questionAnswer = 1;
                }
                else if (UserInput.Instance.AnswerRightTriggered)
                {
                    questionAnswer = 2;
                }

                // If we have recieved an answer
                if (questionAnswer != -1)
                {
                    forceTransition = true;
                    UpdateResult();
                    Debug.Log(string.Join(", ", result));
                }
            }
            else if (curState == State.ShowingHeartPuzzle)
            {
                if (UserInput.Instance.HeartPuzzleInputTriggered)
                {
                    forceTransition = true;
                    heartAudio.Play();
                }
            }
            else if (curState == State.ShowingFinalPuzzle)
            {
                if (UserInput.Instance.FinalPuzzleInputBeingHeld)
                {
                    if (!finalPuzzleAudio.isPlaying)
                        finalPuzzleAudio.Play();

                    finalPuzzleTime += Time.deltaTime;
                    if (finalPuzzleTime >= finalPuzzleCompleteTime)
                        forceTransition = true;
                }
                else
                {
                    // TODO(lewis): add grace period...

                    finalPuzzleTime = 0;
                    finalPuzzleAudio.Stop();
                }
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
                welcomeDisplay.gameObject.SetActive(false);

                if (DEBUG_startWithFinalPuzzle)
                {
                    ShowFinalPuzzle();
                    break;
                }

                if (DEBUG_startWithHeartPuzzle)
                {
                    ShowHeartPuzzle();
                    break;
                }

                ShowQuestion();
                break;

            case State.ShowingQuestion:
                 ShowAnswer();
                break;

            case State.ShowingAnswer:
                curQuestion++;
                if (curQuestion == whenIsTheHeartPuzzle)
                {
                    questionDisplay.gameObject.SetActive(false);
                    ShowHeartPuzzle();
                }
                else if (curQuestion < questions.Count)
                {
                    ShowQuestion();
                }
                else
                {
                    questionDisplay.gameObject.SetActive(false);
                    ShowFinalPuzzle();
                }
                break;

            case State.ShowingHeartPuzzle:
                ShowHeartPuzzleSloved();
                break;

            case State.ShowingHeartPuzzleSolved:
                heartPuzzleDisplay.gameObject.SetActive(false);
                if (curQuestion < questions.Count)
                {
                    ShowQuestion();
                }
                else
                {
                    ShowFinalPuzzle();
                }
                break;

            case State.ShowingFinalPuzzle:
                finalPuzzleDisplay.gameObject.SetActive(false);
                ShowResult();
                break;

            case State.ShowingResult:
                // do nothing...
                break;
        }
    }

    private void ShowQuestion()
    {
        questionDisplay.gameObject.SetActive(true);

        curState = State.ShowingQuestion;
        questionAnswer = -1;
        questionDisplay.ShowQuestion(questions[curQuestion]);
        nextTransition = float.MaxValue;
        blockInputUntil = Time.time + 3.5f;
    }

    private void ShowAnswer()
    {
        curState = State.ShowingAnswer;
        questionDisplay.ShowAnswer(questionAnswer);
        nextTransition = Time.time + 4;
        selectionAudio.Play();
    }

    private void ShowHeartPuzzle()
    {
        heartPuzzleDisplay.gameObject.SetActive(true);

        curState = State.ShowingHeartPuzzle;
        heartPuzzleDisplay.ShowPrompt();
        nextTransition = float.MaxValue;
        blockInputUntil = Time.time + 1.5f;
    }

    private void ShowHeartPuzzleSloved()
    {
        curState = State.ShowingHeartPuzzleSolved;
        heartPuzzleDisplay.ShowSolved();
        nextTransition = Time.time + 3;
        //heartAudio.Play();
    }

    private void ShowFinalPuzzle()
    {
        finalPuzzleDisplay.gameObject.SetActive(true);
        curState = State.ShowingFinalPuzzle;
        nextTransition = float.MaxValue;
    }

    private void ShowResult()
    {
        resultDisplay.gameObject.SetActive(true);

        curState = State.ShowingResult;

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
