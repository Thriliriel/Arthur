using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadCTRL : MonoBehaviour
{
    private Vector3 defaultPosition;
    private float xLimit, yLimit;

    private void Awake()
    {
        defaultPosition = transform.position;
        xLimit = 4.5f;
        yLimit = 4.5f;
    }

    public void FollowMouse(float xValue, float yValue)
    {
        Vector3 position;

        position = new Vector3(0f, 0f, 0f);

        // Verifies horizontal head limits
        if (Mathf.Abs(xValue) < xLimit) position.x = xValue;
        else position.x = xLimit * Mathf.Sign(xValue);

        // Verifies vertical head limits
        if (Mathf.Abs(yValue) < yLimit) position.y = yValue;
        else position.y = yLimit * Mathf.Sign(yValue);

        // Applies transformation into ECA's head
        //transform.position = position;
    }

    public void DefaultPosition()
    {
        transform.position = defaultPosition;
    }
}
