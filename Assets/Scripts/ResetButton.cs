using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ResetButton))]
public class ResetButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ResetButton myTarget = (ResetButton)target;

        if (GUILayout.Button("Reset"))
        {
            myTarget.resettable.ResetNow();
        }
    }
}
#endif

public class ResetButton : MonoBehaviour {

    public Resettable resettable;
    public float resetTime = 2.0f;

    Collider touching = null;
    float touchedAt = 0f;

    Vector3 readyPos = new Vector3(), pressedPos = new Vector3();
    float pressSpeed = 0.25f;

    bool hasReset = false;

	// Use this for initialization
	void Start ()
    {
        readyPos = transform.position;
        pressedPos = transform.position - new Vector3(0, 0.1f, 0.0f);
	}

    IEnumerator Press(bool _down)
    {
        var eoff = new WaitForFixedUpdate();

        Vector3 a = readyPos;
        Vector3 b = pressedPos;
        if (!_down)
        {
            a = pressedPos;
            b = readyPos;
        }

        float dist = (a - b).magnitude;

        for (float t = 0; t < resetTime; t += Time.unscaledDeltaTime)
        {
            float it = Mathf.Clamp01(t / resetTime);
            transform.position = Vector3.Lerp(a, b, it);
            yield return eoff;
        }

        transform.position = b;
        //yield return eoff;
        //yield break;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(string.Format("Hit by {0}, tag {1}, layer {2}", other.name,
            other.tag, other.gameObject.layer));
        if (!touching && other.tag == "Hands")
        {
            touching = other;
            touchedAt = Time.unscaledTime;
            hasReset = false;
            StartCoroutine(Press(true));
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (touching && other == touching && !hasReset)
        {
            float dt = Time.unscaledTime - touchedAt;

            if (dt >= this.resetTime)
            {
                resettable.StartCoroutine(resettable.RealReset(resetTime));
                hasReset = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (touching && other == touching)
        {
            touching = null;
            if (hasReset)
            {
                StartCoroutine(Press(false));
            }
        }
    }
}
