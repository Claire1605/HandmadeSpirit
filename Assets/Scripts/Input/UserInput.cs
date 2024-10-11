using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static UserInput Instance;

    public bool FinalPuzzleInputBeingHeld => _finalPuzzleInput.IsPressed();
    public bool HeartPuzzleInputTriggered => _heartPuzzleInput.triggered;
    public bool AnswerLeftTriggered => _answerLeft.triggered;
    public bool AnswerCenterTriggered => _answerCenter.triggered;
    public bool AnswerRightTriggered => _answerRight.triggered;
    public bool ForcedRestartTriggered => _forcedReset.triggered;

    public bool AnyAnswerTriggered { get { return AnswerLeftTriggered || AnswerCenterTriggered || AnswerRightTriggered; } }

    private PlayerInput _playerInput;

    private InputAction _finalPuzzleInput;
    private InputAction _heartPuzzleInput;
    private InputAction _answerLeft;
    private InputAction _answerCenter;
    private InputAction _answerRight;
    private InputAction _forcedReset;

    //Input Manager
    public static bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else //not in original tutorial
        {
            Destroy(this);
        }

        _playerInput = GetComponent<PlayerInput>();

        SetupInputActions();
    }

    private void SetupInputActions()
    {
        _finalPuzzleInput = _playerInput.actions["FinalPuzzleInput"];
        _heartPuzzleInput = _playerInput.actions["HeartPuzzleInput"];
        _answerRight = _playerInput.actions["AnswerRight"];
        _answerCenter = _playerInput.actions["AnswerCenter"];
        _answerLeft = _playerInput.actions["AnswerLeft"];
        _forcedReset = _playerInput.actions["ForcedReset"];
    }
}