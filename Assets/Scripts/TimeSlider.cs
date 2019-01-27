using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSlider : MonoBehaviour {

    // Min-max range for time scale value
    public Vector2 MinMax = new Vector2(0.0f, 2.0f);
    // For showing in debugger
    public float TimeDisplay;
    public float RefreshRate = 1.0f / 90.0f;
    Vector3 minPos = Vector3.zero;
    Vector3 maxPos = Vector3.zero;

    Rigidbody rb;

    public float TimeScale
    {
        get
        {
            return (transform.position - minPos).magnitude / transform.localScale.x;
        }
    }

    private void Start()
    {
        var xoffset = transform.right * transform.localScale.x * 0.5f;
        minPos = transform.position - xoffset;
        maxPos = transform.position + xoffset;
        rb = GetComponent<Rigidbody>();
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = RefreshRate;
    }

    private void LateUpdate()
    {

    }

    void FixedUpdate ()
    {
        var TimeDisplay = TimeScale;

        if (TimeDisplay < MinMax.x)
        {
            transform.position = minPos;
            TimeDisplay = MinMax.x;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else if (TimeDisplay > MinMax.y)
        {
            transform.position = maxPos;
            TimeDisplay = MinMax.y;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (TimeDisplay != Time.timeScale)
        {
            Time.timeScale = TimeDisplay;
        }

        Time.fixedDeltaTime = RefreshRate * Time.timeScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(minPos, 0.005f);
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(maxPos, 0.005f);
    }
}
