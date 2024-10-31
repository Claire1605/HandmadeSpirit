using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraColourFlickerer
{
    public Camera camera;
    public Color colour;

    public float flickerSize = 0.2f;
    public float flickerRate = 1.0f;

    private float t = 0.0f;

    public void Reset()
    {
        camera.backgroundColor = Color.black;
        t = 0;
    }

    public void Update()
    {
        t += Time.deltaTime * flickerRate;
        float trunk = t - Mathf.Floor(t);

        if (trunk < flickerSize)
        {
            if (camera.backgroundColor != colour) camera.backgroundColor = colour;
        }
        else
        {
            if (camera.backgroundColor != Color.black) camera.backgroundColor = Color.black;
        }
    }
}

public class GameController : MonoBehaviour
{
    public List<TextAsset> questionsCSVs;
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
    public float finalPuzzleConnectionBreakGraceTime = 1;
    public bool useFinalPuzzleConnectionBreakGraceTimer = true;
    public float finalPuzzleSlowFlickerRate = 0.6f;
    public float finalPuzzleFastFlickerRate = 1.5f;
    public float showResultShowTime = 6;

    public bool DEBUG_startWithHeartPuzzle = false;
    public bool DEBUG_startWithFinalPuzzle = false;
    [Range(0, 10.0f)] public float DEBUG_timeScale = 1.0f;

    public AudioSource selectionAudio;
    public AudioSource heartAudio;
    public AudioSource finalPuzzleAudio;
    public AudioSource resultAudio;

    private Transform welcomeDisplay;
    private List<List<Question>> allQuestionsLists = new();
    private int activeQuestionListIndex;
    private List<Question> ActiveQuestionsList { get { return allQuestionsLists[activeQuestionListIndex]; } }
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
        ShowingRestart,
    }

    private int curQuestion = 0;
    private State curState = State.ShowingWelcome;
    private int questionAnswer = -1;

    private float nextTransition = float.MaxValue;
    private float blockInputUntil = float.MinValue;

    private float finalPuzzleTimer;
    private float finalPuzzleConnectionBreakGraceTimer;
    private bool FinalPuzzleGraceTimerActive
    {
        get {
            return useFinalPuzzleConnectionBreakGraceTimer && finalPuzzleConnectionBreakGraceTimer > 0f;
        }
    }
    private float showResultTimer;

    private int[] result = new int[QuestionHelper.NUMBER_OF_SPRITS];
    CameraColourFlickerer cameraColourFlickerer = new();

    private void Start()
    {
        for (int i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }

        foreach (var questionCSV in questionsCSVs)
        {
            allQuestionsLists.Add(QuestionHelper.GetQuestionsFromCSV(questionCSV));
            foreach (var q in allQuestionsLists.Last())
            {
                Debug.Log(q.ToString());
            }
        }

        // NOTE(lewis): random roll starting question set
        activeQuestionListIndex = Random.Range(0, allQuestionsLists.Count);

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

        cameraColourFlickerer.camera = resultCamera;
    }

    private void Update()
    {
        Time.timeScale = DEBUG_timeScale;

        if (UserInput.Instance.ForcedRestartTriggered)
        {
            ResetGame();
            return;
        }

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
                // NOTE(lewis): use a grace period to catch short gaps in the input
                if (FinalPuzzleGraceTimerActive)
                {
                    finalPuzzleConnectionBreakGraceTimer -= Time.deltaTime;
                }

                if (UserInput.Instance.FinalPuzzleInputBeingHeld || FinalPuzzleGraceTimerActive)
                {
                    finalPuzzleConnectionBreakGraceTimer = finalPuzzleConnectionBreakGraceTime;

                    if (!finalPuzzleAudio.isPlaying)
                        finalPuzzleAudio.Play();

                    finalPuzzleTimer += Time.deltaTime;
                    if (finalPuzzleTimer >= finalPuzzleCompleteTime)
                        forceTransition = true;
                    
                    {
                        var resultSpiritIndex = result.ToList().IndexOf(result.Max());
                        var spirit = spirits[resultSpiritIndex];
                        cameraColourFlickerer.colour = spirit.colour;
                    }

                    float puzzleT = finalPuzzleTimer / finalPuzzleCompleteTime;
                    float kShowUntilT = 0.9f;
                    if (puzzleT < kShowUntilT)
                    {
                        cameraColourFlickerer.flickerRate = Mathf.Lerp(finalPuzzleSlowFlickerRate, finalPuzzleFastFlickerRate, (puzzleT / kShowUntilT));
                        cameraColourFlickerer.Update();
                    }
                    else
                    {
                        cameraColourFlickerer.Reset();
                    }
                }
                else
                {
                    finalPuzzleConnectionBreakGraceTimer = 0f;
                    finalPuzzleTimer = 0;
                    finalPuzzleAudio.Stop();
                    cameraColourFlickerer.Reset();
                }
            }
            else if (curState == State.ShowingResult)
            {
                showResultTimer += Time.deltaTime;
                if (showResultTimer >= showResultShowTime)
                    forceTransition = true;
            }
            else if (curState == State.ShowingRestart)
            {
                if (UserInput.Instance.AnyAnswerTriggered)
                {
                    forceTransition = true;
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
            answerResult = ActiveQuestionsList[curQuestion].answer0Result;
        }
        else if (questionAnswer == 1)
        {
            answerResult = ActiveQuestionsList[curQuestion].answer1Result;
        }
        else if (questionAnswer == 2)
        {
            answerResult = ActiveQuestionsList[curQuestion].answer2Result;
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
        float qProgress = (float)(curQuestion + 1) / ActiveQuestionsList.Count;

        questionDisplay.oldColour = resultCamera.backgroundColor;
        questionDisplay.newColour = new Color(c.r * qProgress, c.g * qProgress, c.b * qProgress, 1);
    }

    private void ResetGame()
    {
        curQuestion = 0;
        curState = State.ShowingWelcome;
        questionAnswer = -1;

        activeQuestionListIndex = (activeQuestionListIndex + 1) % allQuestionsLists.Count;

        nextTransition = float.MaxValue;
        blockInputUntil = float.MinValue;

        result = new int[QuestionHelper.NUMBER_OF_SPRITS];
        finalPuzzleTimer = 0.0f;
        finalPuzzleConnectionBreakGraceTimer = 0.0f;
        showResultTimer = 0.0f;

        welcomeDisplay.gameObject.SetActive(true);
        questionDisplay.gameObject.SetActive(false);
        heartPuzzleDisplay.gameObject.SetActive(false);
        resultDisplay.gameObject.SetActive(false);
        finalPuzzleDisplay.gameObject.SetActive(false);

        resultCamera.backgroundColor = Color.black;

        heartAudio.Stop();
        cameraColourFlickerer.Reset();
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
                else if (curQuestion < ActiveQuestionsList.Count)
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
                if (curQuestion < ActiveQuestionsList.Count)
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
                ShowRestart();
                break;

            case State.ShowingRestart:
                ResetGame();
                break;
        }
    }

    private void ShowQuestion()
    {
        questionDisplay.gameObject.SetActive(true);

        curState = State.ShowingQuestion;
        questionAnswer = -1;
        questionDisplay.ShowQuestion(ActiveQuestionsList[curQuestion]);
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

    private void ShowRestart()
    {
        curState = State.ShowingRestart;
        resultDisplay.ShowRestartText();
    }
}
