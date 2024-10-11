using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleInput : MonoBehaviour
{
    void Update()
    {
        if (UserInput.Instance.FinalPuzzleInputBeingHeld)
        {
            Debug.Log("Final [held]");
        }
        
        if (UserInput.Instance.HeartPuzzleInputTriggered)
        {
            Debug.Log("Heart");
        }

        if (UserInput.Instance.AnswerLeftTriggered)
        {
            Debug.Log("Left");
        }

        if (UserInput.Instance.AnswerCenterTriggered)
        {
            Debug.Log("Center");
        }

        if (UserInput.Instance.AnswerRightTriggered)
        {
            Debug.Log("Right");
        }

        if (UserInput.Instance.ForcedRestartTriggered)
        {
            Debug.Log("Forced restart");
        }
    }
}
