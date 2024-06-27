using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static UserInput Instance;

    public Vector2 NavigationInput { get; private set; }
    public bool InteractJustPressed { get; private set;}
    public bool InteractBeingHeld { get; private set; }
    public bool InteractReleased { get; private set; }
    public bool PauseInput { get; private set; }
    public bool DebugInput { get; private set; }
    public bool MouseClick { get; private set; }

    private PlayerInput _playerInput;

    private InputAction _navigationAction;
    private InputAction _interactAction;
    private InputAction _pauseAction;
    private InputAction _debugAction;
    private InputAction _mouseClickAction;

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

    void Update()
    {
        UpdateInputs();
    }

    private void SetupInputActions()
    {
        _navigationAction = _playerInput.actions["MOVEMENT"];
        _interactAction = _playerInput.actions["INTERACT"];
        _pauseAction = _playerInput.actions["PAUSE"];
        _debugAction = _playerInput.actions["DEBUG"];
        _mouseClickAction = _playerInput.actions["MOUSE_CLICK"];
    }

    private void UpdateInputs()
    {
        NavigationInput = _navigationAction.ReadValue<Vector2>();
        InteractJustPressed = _interactAction.WasPressedThisFrame();
        InteractBeingHeld = _interactAction.IsPressed();
        InteractReleased = _interactAction.WasReleasedThisFrame();
        PauseInput = _pauseAction.WasPressedThisFrame();
        DebugInput = _debugAction.WasPressedThisFrame();
        MouseClick = _mouseClickAction.IsPressed();
    }
}