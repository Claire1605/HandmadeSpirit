using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleInput : MonoBehaviour
{
    void Update()
    {
        if (UserInput.Instance.NavigationInput.x > 0)
        {
            Debug.Log("Right");
        }
        else if (UserInput.Instance.NavigationInput.x < 0)
        {
            Debug.Log("Left");
        }

        if (UserInput.Instance.NavigationInput.y > 0)
        {
            Debug.Log("Up");
        }
        else if (UserInput.Instance.NavigationInput.y < 0)
        {
            Debug.Log("Down");
        }

        if (UserInput.Instance.InteractJustPressed)
        {
            Debug.Log("Space");
        }

        if (UserInput.Instance.MouseClick)
        {
            Debug.Log("Click");
        }
    }
}
